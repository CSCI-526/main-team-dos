using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginManager : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // Constants
    // ───────────────────────────────────────────────
    private const string SUPABASE_API_KEY =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo";
    private const string SUPABASE_URL = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/users";

    // ───────────────────────────────────────────────
    // UI References
    // ───────────────────────────────────────────────
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text feedbackText;

    // ───────────────────────────────────────────────
    // Serializable Data Classes
    // ───────────────────────────────────────────────
    [System.Serializable]
    private class UserData
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    private class UserArray
    {
        public UserData[] users;
        public static UserArray FromJson(string json) =>
            JsonUtility.FromJson<UserArray>("{\"users\":" + json + "}");
    }

    // ───────────────────────────────────────────────
    // Unity Lifecycle
    // ───────────────────────────────────────────────
    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterClicked);
        loginButton.onClick.AddListener(OnLoginClicked);
        feedbackText.text = string.Empty;
    }

    private void OnDestroy()
    {
        registerButton.onClick.RemoveListener(OnRegisterClicked);
        loginButton.onClick.RemoveListener(OnLoginClicked);
    }

    // ───────────────────────────────────────────────
    // UI Event Handlers
    // ───────────────────────────────────────────────
    private void OnRegisterClicked()
    {
        if (!ValidateInputs()) return;
        StartCoroutine(RegisterUser(usernameInput.text, passwordInput.text));
    }

    private void OnLoginClicked()
    {
        if (!ValidateInputs()) return;
        StartCoroutine(LoginUser(usernameInput.text, passwordInput.text));
    }

    // ───────────────────────────────────────────────
    // Network Operations
    // ───────────────────────────────────────────────
    private IEnumerator RegisterUser(string username, string password)
    {
        ShowFeedback("Registering...");

        string jsonPayload = JsonUtility.ToJson(new UserData { username = username, password = password });
        using UnityWebRequest www = new UnityWebRequest(SUPABASE_URL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("apikey", SUPABASE_API_KEY);
        www.SetRequestHeader("Prefer", "return=minimal");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            PlayerPrefs.SetString("CurrentUsername", username);
            PlayerPrefs.Save();
            ShowFeedback($"Registration successful! Welcome, {username}.");
            LoadStarterScene();
        }
        else if (www.responseCode == 409)
        {
            ShowFeedback($"Registration failed: username '{username}' already exists.");
        }
        else
        {
            ShowFeedback($"Registration failed: {www.error}");
            Debug.LogError($"Supabase Registration Error: {www.error} | Response: {www.downloadHandler.text}");
        }
    }

    private IEnumerator LoginUser(string username, string password)
    {
        ShowFeedback("Logging in...");

        string queryURL = $"{SUPABASE_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&password=eq.{UnityWebRequest.EscapeURL(password)}";
        using UnityWebRequest www = UnityWebRequest.Get(queryURL);

        www.SetRequestHeader("apikey", SUPABASE_API_KEY);
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            ShowFeedback($"Login failed: network error ({www.error})");
            Debug.LogError($"Supabase Login Network Error: {www.error}");
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(www.downloadHandler.text) && www.downloadHandler.text != "[]")
        {
            PlayerPrefs.SetString("CurrentUsername", username);
            PlayerPrefs.Save();
            ShowFeedback($"Login successful! Welcome, {username}.");
            LoadStarterScene();
        }
        else
        {
            ShowFeedback("Invalid username or password.");
        }
    }

    // ───────────────────────────────────────────────
    // Helpers
    // ───────────────────────────────────────────────
    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowFeedback("Please enter both username and password!");
            return false;
        }
        return true;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
        else
            Debug.Log(message);
    }

    private void LoadStarterScene() => SceneManager.LoadScene("Starter");
}
