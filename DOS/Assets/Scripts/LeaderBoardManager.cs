using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class LeaderboardEntry
{
    public string username;
    public int level;
    public float time;
}

public class LeaderBoardManager : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/leaderboard";
    public string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo";

    [Header("Level Selection UI")]
    public TMP_Dropdown levelDropDown;

    [Header("UI References")]
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] timeTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] rankTexts = new TextMeshProUGUI[5];

    [Header("Current Player Info")]
    public TextMeshProUGUI playerRankText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerTimeText;

    [Header("Search Input")]
    public TMP_InputField searchInput;
    [Header("Friend Info Display")]
    public TextMeshProUGUI friendRankText;
    public TextMeshProUGUI friendNameText;
    public TextMeshProUGUI friendTimeText;


    private int currentLevel = 0;
    private int currentPage = 0;
    private int entriesPerPage = 5;
    private List<LeaderboardEntry> allEntries = new List<LeaderboardEntry>();


    private void Start()
    {
        if (levelDropDown == null)
        {
            Debug.LogError("Level Dropdown reference is missing.");
            return;
        }
        if (searchInput != null)
        {
            searchInput.onSubmit.AddListener(OnSearchSubmitted);
        }


        levelDropDown.onValueChanged.AddListener(_ => OnLevelDropDownChanged());
        _ = LoadLeaderboardAsync();
    }
    
    private void OnSearchSubmitted(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.Log("Search input empty.");
            return;
        }

        // Find friend in current leaderboard
        var entry = allEntries.Find(e => e.username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            Debug.Log($"Found {username}: time = {entry.time}");

            friendRankText.text = "#" + (allEntries.IndexOf(entry) + 1);
            friendNameText.text = entry.username;
            friendTimeText.text = FormatTime(entry.time);
        }
        else
        {
            Debug.Log($"{username} not found in current leaderboard.");
            friendRankText.text = "#--";
            friendNameText.text = username;
            friendTimeText.text = "--:--.--";
        }

        // Optional: clear input
        searchInput.text = "";
        searchInput.DeactivateInputField();
    }



    private void OnLevelDropDownChanged()
    {
        string selectedText = levelDropDown.options[levelDropDown.value].text;
        string levelString = selectedText.Replace("Level ", "").Trim();
        if (int.TryParse(levelString, out int levelNumber))
        {
            currentLevel = levelNumber;
            currentPage = 0;
            _ = LoadLeaderboardAsync(); 
        }
        else
        {
            Debug.LogError("Could not parse level number: " + selectedText);
        }
    }

    private async Task LoadLeaderboardAsync()
    {
        if (currentLevel == 0 && levelDropDown != null)
            currentLevel = levelDropDown.value + 1;

        try
        {
            Debug.Log($"Fetching leaderboard for Level {currentLevel}...");

            string url = $"{supabaseUrl}?level=eq.{currentLevel}&order=time.asc";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("apikey", apiKey);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                allEntries = JsonHelper.FromJson<LeaderboardEntry>(json);

                DisplayCurrentPage();
                DisplayCurrentPlayerRank(allEntries);

                Debug.Log($"Leaderboard loaded successfully for Level {currentLevel}");
            }
            else
            {
                Debug.LogError($"Failed to fetch leaderboard: {request.error}");
                ShowErrorMessage();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while fetching leaderboard: {ex.Message}");
            ShowErrorMessage();
        }
    }

    private void DisplayCurrentPage()
    {
        int startIndex = currentPage * entriesPerPage;

        for (int i = 0; i < entriesPerPage; i++)
        {
            int entryIndex = startIndex + i;

            if (entryIndex < allEntries.Count)
            {
                rankTexts[i].text = "#" + (entryIndex + 1);
                nameTexts[i].text = allEntries[entryIndex].username;
                timeTexts[i].text = FormatTime(allEntries[entryIndex].time);
            }
            else
            {
                rankTexts[i].text = "#" + (entryIndex + 1);
                nameTexts[i].text = "---";
                timeTexts[i].text = "--:--.--";
            }
        }
    }

    public void OnPageUp()
    {
        if (currentPage > 0)
        {
            currentPage--; DisplayCurrentPage();
            Debug.Log($"Page Up → Now showing page {currentPage}");
        }
        else { Debug.Log($"Already at first page! {currentPage}"); }
    } 
    
    public void OnPageDown() {
        int maxPage = Mathf.CeilToInt((float)allEntries.Count / entriesPerPage) - 1;
        if (currentPage < maxPage)
        {
            currentPage++; DisplayCurrentPage();
            Debug.Log($"Page Down → Now showing page {currentPage}");
        }
        else
        {
            Debug.Log($"Already at last page! {currentPage}");
        }
    }


    private void DisplayCurrentPlayerRank(List<LeaderboardEntry> entries)
    {
        string currentPlayerUsername = PlayerPrefs.GetString("CurrentUsername", "Speed");
        int playerIndex = entries.FindIndex(e => e.username == currentPlayerUsername);

        if (playerIndex >= 0)
        {
            var entry = entries[playerIndex];
            playerRankText.text = "#" + (playerIndex + 1);
            playerNameText.text = entry.username;
            playerTimeText.text = FormatTime(entry.time);
        }
        else
        {
            playerRankText.text = "#--";
            playerNameText.text = currentPlayerUsername;
            playerTimeText.text = "--:--.--";
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int sec = Mathf.FloorToInt(seconds % 60);
        int ms = Mathf.FloorToInt((seconds * 100) % 100);
        return $"{minutes:00}:{sec:00}.{ms:00}";
    }

    private void ShowErrorMessage()
    {
        for (int i = 0; i < entriesPerPage; i++)
        {
            nameTexts[i].text = "Failed to load";
            timeTexts[i].text = "--:--.--";
        }
    }
}

public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}
