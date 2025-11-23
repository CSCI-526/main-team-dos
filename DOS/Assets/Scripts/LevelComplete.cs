using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class LevelComplete : MonoBehaviour
{
    private const string SUPABASE_API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo";
    private const string LEADERBOARD_URL = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/leaderboard";
    private const string ATTEMPTS_URL = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/level_attempts";

    public int CURRENT_LEVEL = 1;

    [Header("UI References")]
    [SerializeField] private Timer timer;
    [SerializeField] private GameObject missionCompletePanel;
    [SerializeField] private TMP_Text missionCompleteText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelCompleteSound;

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public int level;
        public float time;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Check if the player just teleported (and is invincible)
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player.IsInvincible)
        {
            // Player is in the post-teleport grace period, ignore the trigger
            return; 
        }

        timer.Stop();
        float finalTime = timer.elapsedTime;

        // --- Play Audio ---
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        Time.timeScale = 0f;
        missionCompletePanel.SetActive(true);
        missionCompleteText.text = "Time: " + timer.ElapsedTimeInString();

        RecordNewTime(finalTime);
    }

    private void RecordNewTime(float newTime)
    {
        string username = PlayerPrefs.GetString("CurrentUsername", "wyatt");
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Cannot record time: Username is missing. Please log in first.");
            return;
        }

        StartCoroutine(CheckAndRecordTime(username, newTime));
    }

    private IEnumerator CheckAndRecordTime(string username, float newTime)
    {
        string getQueryURL = $"{LEADERBOARD_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&level=eq.{CURRENT_LEVEL}&select=time";

        using (UnityWebRequest request = UnityWebRequest.Get(getQueryURL))
        {
            request.SetRequestHeader("apikey", SUPABASE_API_KEY);
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Leaderboard GET Error: {request.error}");
                yield break;
            }

            string response = request.downloadHandler.text;
            bool isFirstAttempt = response.Length <= 2 || response == "[]";

            if (isFirstAttempt)
            {
                StartCoroutine(PostNewTime(username, newTime));
            }
            else
            {
                float existingTime = GetTimeFromJson(response);
                if (newTime < existingTime)
                {
                    StartCoroutine(PutNewTime(username, newTime));
                }
            }
            StartCoroutine(LogLevelAttempt(username, newTime));
        }
    }

    private float GetTimeFromJson(string json)
    {
        try
        {
            string innerJson = json.Substring(1, json.Length - 2);
            int timeStart = innerJson.IndexOf("\"time\":") + 7;
            string timeStr = innerJson.Substring(timeStart).TrimEnd('}');
            return float.Parse(timeStr);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse time from JSON: {e.Message}");
            return float.MaxValue;
        }
    }

    private IEnumerator PostNewTime(string username, float newTime)
    {
        LeaderboardEntry newEntry = new LeaderboardEntry { username = username, level = CURRENT_LEVEL, time = newTime };
        string jsonPayload = JsonUtility.ToJson(newEntry);

        using (UnityWebRequest request = new UnityWebRequest(LEADERBOARD_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", SUPABASE_API_KEY);
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"POST Failed: {request.error}");
            }
        }
    }

    private IEnumerator PutNewTime(string username, float newTime)
    {
        string patchQueryURL = $"{LEADERBOARD_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&level=eq.{CURRENT_LEVEL}";
        string jsonPayload = $"{{\"time\":{newTime}}}";

        using (UnityWebRequest request = new UnityWebRequest(patchQueryURL, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", SUPABASE_API_KEY);
            request.SetRequestHeader("Prefer", "return=representation");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PATCH Failed: {request.error}");
            }
        }
    }

    private IEnumerator LogLevelAttempt(string username, float newTime)
    {
        LeaderboardEntry attemptEntry = new LeaderboardEntry { username = username, level = CURRENT_LEVEL, time = newTime };
        string jsonPayload = JsonUtility.ToJson(attemptEntry);

        using (UnityWebRequest request = new UnityWebRequest(ATTEMPTS_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", SUPABASE_API_KEY);
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Attempt Log Failed: {request.error}");
            }
        }
    }
}