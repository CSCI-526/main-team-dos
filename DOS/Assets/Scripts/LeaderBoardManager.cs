using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Needed for TextMeshProUGUI and TMP_Dropdown

// NOTE: LeaderboardEntry and JsonHelper classes remain the same as the previous response.
// They are omitted here for brevity. You must keep them in your file.
[System.Serializable]
public class LeaderboardEntry
{
    // Supabase column names are lowercase
    public string username; // Corresponds to 'username'
    public int level;       // Corresponds to 'level'
    public float time;      // Corresponds to 'time'
    // id and created_at can be omitted if you don't need them
}

public class LeaderBoardManager : MonoBehaviour
{
    [Header("Supabase Settings")]
    public string supabaseUrl = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/leaderboard";
    public string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo"; 

    // NEW: Reference to your Dropdown UI element
    [Header("Level Selection UI")]
    public TMP_Dropdown levelDropDown;
    
    // The current level is now derived from the dropdown value
    private int currentLevel; 
    
    [Header("UI References")]
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] timeTexts = new TextMeshProUGUI[5];
    public TextMeshProUGUI[] rankTexts = new TextMeshProUGUI[5];

    void Start()
    {
       
        // Attach a listener to the dropdown so LoadLeaderboard runs whenever the value changes.
        if (levelDropDown != null)
        {
            // The listener takes the index (0-based) as an argument.
            levelDropDown.onValueChanged.AddListener(delegate {
                OnLevelDropDownChanged(levelDropDown);
            });
            // Load the board initially with the default/starting dropdown value
            LoadLeaderboard(); 
        }
        else
        {
            Debug.LogError("Level Dropdown reference is missing on the LeaderboardManager.");
        }
    }

    // Inside the LeaderboardManager class
    public void OnLevelDropDownChanged(TMP_Dropdown dropdown)
    {
        // 1. Get the text of the currently selected option
        string selectedText = dropdown.options[dropdown.value].text;

        // 2. Extract the number from the text (e.g., turn "Level 3" into "3")
        // This assumes the label is always "Level [Number]"
        string levelString = selectedText.Replace("Level ", "").Trim();

        // 3. Convert the string number to an integer
        if (int.TryParse(levelString, out int levelNumber))
        {
            // 4. Set the current level and load the board
            currentLevel = levelNumber; 
            LoadLeaderboard();
        }
        else
        {
            Debug.LogError("Could not parse level number from dropdown text: " + selectedText);
        }
    }
    
    // LoadLeaderboard now simply executes the fetch routine.
    public void LoadLeaderboard()
    {
        // If not using the dropdown (e.g., calling from Start before listener attached)
        if (currentLevel == 0 && levelDropDown != null)
        {
             // Initialize currentLevel from the dropdown's starting value
             currentLevel = levelDropDown.value + 1;
        }
        
        StartCoroutine(FetchLeaderboard());
    }

    IEnumerator FetchLeaderboard()
    {
        // Ensure a valid level has been set
        if (currentLevel < 1) yield break; 
        
        // Build the URL using the currentLevel determined by the dropdown.
        string url = $"{supabaseUrl}?level=eq.{currentLevel}&order=time.asc&limit=5";
        
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // ... (rest of success logic remains the same)
            string jsonResponse = request.downloadHandler.text;
            List<LeaderboardEntry> entries = JsonHelper.FromJson<LeaderboardEntry>(jsonResponse);
            DisplayLeaderboard(entries);
        }
        else
        {
            Debug.LogError($"Failed to fetch leaderboard for Level {currentLevel}: {request.error}");
            ShowErrorMessage();
        }
    }

    // (The rest of the code is unchanged and can remain the same)
    void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
        // ... (use entry.username instead of entry.playerName)
        for (int i = 0; i < 5; i++)
        {
            if (i < entries.Count)
            {
                // Show entry
                rankTexts[i].text = "#" + (i + 1);
                nameTexts[i].text = entries[i].username; // Use username
                timeTexts[i].text = FormatTime(entries[i].time);
            
            }
            else
            {
                // Empty slot
                rankTexts[i].text = "#" + (i + 1);
                nameTexts[i].text = "---";
                timeTexts[i].text = "--:--.--";
            }
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100);
        
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    void ShowErrorMessage()
    {
        for (int i = 0; i < 5; i++)
        {
            nameTexts[i].text = "Failed to load";
            timeTexts[i].text = "--:--.--";
        }
    }
}

// Helper class to handle JSON array deserialization in Unity
public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        // Wrap the array in a temporary object format that JsonUtility understands
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}