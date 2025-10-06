using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class LoginManager : MonoBehaviour
{
    private const string SUPABASE_API_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRoYW9qeG1xcXZwanR6aXJvd2JkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk1OTczODMsImV4cCI6MjA3NTE3MzM4M30.PkARXX-cXJ5PVVOS10EqT6OynHOML_yIMPEg-jh9-Qo";
    private const string SUPABASE_URL = "https://dhaojxmqqvpjtzirowbd.supabase.co/rest/v1/users";

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button registerButton;
    public Button loginButton; 
    public TMP_Text feedbackText;


    [System.Serializable]
    public class UserData
    {
        public string username;
        public string password;
    }
    
    [System.Serializable]
    public class UserArray
    {
        public UserData[] users;

        public static UserArray FromJson(string json)
        {
            string wrapper = "{\"users\":" + json + "}";
            return JsonUtility.FromJson<UserArray>(wrapper);
        }
    }


    void Start()
    {
        registerButton.onClick.AddListener(OnRegisterClicked);
        loginButton.onClick.AddListener(OnLoginClicked); 

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void OnRegisterClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Please enter both username and password!");
            return;
        }

        StartCoroutine(RegisterUser(username, password));
    }

    IEnumerator RegisterUser(string username, string password)
    {
        ShowFeedback("Registering...");
        UserData newUser = new UserData { username = username, password = password };
        string jsonPayload = JsonUtility.ToJson(newUser);

        using (UnityWebRequest www = new UnityWebRequest(SUPABASE_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", SUPABASE_API_KEY);
            www.SetRequestHeader("Prefer", "return=minimal"); 

            yield return www.SendWebRequest(); 

            if (www.result == UnityWebRequest.Result.Success)
            {
                ShowFeedback($"Registration successful for user: {username}! Loading scene...");
                PlayerPrefs.SetString("CurrentUsername", username); // 🔑 set active session user
                PlayerPrefs.Save();
                LoadStarterScene();
            }
            else if (www.responseCode == 409) 
            {
                ShowFeedback($"Registration failed: Username '{username}' already exists.");
            }
            else
            {
                ShowFeedback($"Registration failed. Error: {www.error}");
                Debug.LogError($"Supabase Registration Error: {www.error} | Response: {www.downloadHandler.text}");
            }
        }
    }

    void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Please enter both username and password!");
            return;
        }

        StartCoroutine(LoginUser(username, password));
    }

    IEnumerator LoginUser(string username, string password)
    {
        ShowFeedback("Logging in...");
        
        string queryURL = $"{SUPABASE_URL}?username=eq.{UnityWebRequest.EscapeURL(username)}&password=eq.{UnityWebRequest.EscapeURL(password)}";
        
        using (UnityWebRequest www = UnityWebRequest.Get(queryURL))
        {
            www.SetRequestHeader("apikey", SUPABASE_API_KEY);
            www.SetRequestHeader("Accept", "application/json");
            
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                ShowFeedback($"Login failed. Network Error: {www.error}");
                Debug.LogError($"Supabase Login Network Error: {www.error}");
                yield break;
            }

            if (www.downloadHandler.text.Length > 2 && www.downloadHandler.text != "[]") 
            {
                ShowFeedback($"Login successful! Welcome, {username}! Loading scene...");
                PlayerPrefs.SetString("CurrentUsername", username);
                PlayerPrefs.Save(); 
                LoadStarterScene();
            }
            else
            {
                ShowFeedback("Login failed: Invalid username or password.");
            }
        }
    }


    void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        else
        {
            Debug.Log(message);
        }
    }

    void LoadStarterScene()
    {
        SceneManager.LoadScene("Starter");
    }

    void OnDestroy()
    {
        registerButton.onClick.RemoveListener(OnRegisterClicked);
        loginButton.onClick.RemoveListener(OnLoginClicked); 
    }
}