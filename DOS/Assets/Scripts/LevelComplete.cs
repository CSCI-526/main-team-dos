using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class LevelComplete : MonoBehaviour
{
    private const string SUPABASE_API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo";
    private const string LEADERBOARD_URL = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/leaderboard";
    
    // For simplicity, assume level is always 1
    private const int CURRENT_LEVEL = 1;

    [Header("UI References")]
    [SerializeField] private Timer timer;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TMP_Text missionCompleteText;

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public int level;
        public float time; 
    }

    [System.Serializable]
    public class SupabaseEntry
    {
        public float time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            timer.Stop();
            
            float finalTime = timer.elapsedTime;

            Time.timeScale = 0f;
            missionCompletePanel.SetActive(true);
            missionCompleteText.text = "Time: " + timer.ElapsedTimeInString();

            RecordNewTime(finalTime);
        }
    }

    private void RecordNewTime(float newTime)
    {
        string username = PlayerPrefs.GetString("CurrentUsername", "");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Cannot record time: Username is missing from PlayerPrefs! Please log in first.");
            return;
        }

        StartCoroutine(CheckAndRecordTime(username, newTime));
    }
    

    IEnumerator CheckAndRecordTime(string username, float newTime)
    {
        string getQueryURL = $"{LEADERBOARD_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&level=eq.{CURRENT_LEVEL}&select=time";
        
        using (UnityWebRequest wwwGet = UnityWebRequest.Get(getQueryURL))
        {
            wwwGet.SetRequestHeader("apikey", SUPABASE_API_KEY);
            wwwGet.SetRequestHeader("Accept", "application/json");

            yield return wwwGet.SendWebRequest();

            if (wwwGet.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Leaderboard GET Error: {wwwGet.error}");
                yield break;
            }

            string responseJson = wwwGet.downloadHandler.text;
            bool isFirstAttempt = (responseJson.Length <= 2 || responseJson == "[]");

            if (isFirstAttempt)
            {
                Debug.Log("First attempt. POSTing new score.");
                StartCoroutine(PostNewTime(username, newTime));
            }
            else
            {
                float existingTime = GetTimeFromJson(responseJson); 
                
                if (newTime < existingTime)
                {
                    Debug.Log($"New time ({newTime}) is better. Updating score.");
                    StartCoroutine(PutNewTime(username, newTime));
                }
                else
                {
                    Debug.Log($"Score not improved. Old time: {existingTime}");
                }
            }
        }
    }

    private float GetTimeFromJson(string json)
    {
        try
        {
            string innerJson = json.Substring(1, json.Length - 2); // Remove array brackets
            SupabaseEntry entry = JsonUtility.FromJson<SupabaseEntry>(innerJson);
            return entry.time;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse time from JSON. Error: {e.Message}");
            return float.MaxValue; 
        }
    }


    IEnumerator PostNewTime(string username, float newTime)
    {
        LeaderboardEntry newEntry = new LeaderboardEntry { username = username, level = CURRENT_LEVEL, time = newTime };
        string jsonPayload = JsonUtility.ToJson(newEntry);
        
        using (UnityWebRequest wwwPost = new UnityWebRequest(LEADERBOARD_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            wwwPost.uploadHandler = new UploadHandlerRaw(bodyRaw);
            wwwPost.downloadHandler = new DownloadHandlerBuffer();
            wwwPost.SetRequestHeader("Content-Type", "application/json");
            wwwPost.SetRequestHeader("apikey", SUPABASE_API_KEY);
            wwwPost.SetRequestHeader("Prefer", "return=minimal"); 

            yield return wwwPost.SendWebRequest();

            if (wwwPost.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully recorded first score: {newTime}.");
            }
            else
            {
                Debug.LogError($"POST Failed. Error: {wwwPost.error}");
            }
        }
    }
    
    IEnumerator PutNewTime(string username, float newTime)
    {
        string patchQueryURL = $"{LEADERBOARD_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&level=eq.{CURRENT_LEVEL}";
        
        string jsonPayload = $"{{\"time\":{newTime}}}";
        
        using (UnityWebRequest wwwPatch = new UnityWebRequest(patchQueryURL, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            wwwPatch.uploadHandler = new UploadHandlerRaw(bodyRaw);
            wwwPatch.downloadHandler = new DownloadHandlerBuffer();
            wwwPatch.SetRequestHeader("Content-Type", "application/json");
            wwwPatch.SetRequestHeader("apikey", SUPABASE_API_KEY);
            wwwPatch.SetRequestHeader("Prefer", "return=representation");
            
            yield return wwwPatch.SendWebRequest();
            
            if (wwwPatch.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully updated score to {newTime}.");
                Debug.Log($"Response: {wwwPatch.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"PATCH Failed. Error: {wwwPatch.error}");
            }
        }
    }
}