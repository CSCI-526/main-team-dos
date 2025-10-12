using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class AmplitudeAnalytics : MonoBehaviour
{
    private const string API_KEY = "5e89e99bb579654bfbc0cd077b04c8e9";
    private const string ENDPOINT = "https://api2.amplitude.com/2/httpapi";
    private const string DEVICE_ID_KEY = "amplitude_device_id";
    
    private static string GetOrCreateDeviceId()
    {
        string deviceId = PlayerPrefs.GetString(DEVICE_ID_KEY, "");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(DEVICE_ID_KEY, deviceId);
            PlayerPrefs.Save();
        }
        return deviceId;
    }


    public void LogEvent(string eventName, Dictionary<string, object> properties = null)
    {
        StartCoroutine(SendEvent(eventName, properties));
    }

    private IEnumerator SendEvent(string eventName, Dictionary<string, object> properties)
    {
        string deviceId = GetOrCreateDeviceId();
        var eventObj = new Dictionary<string, object>
        {
            { "user_id", "webuser" },
            { "device_id", deviceId },
            { "event_type", eventName },
            { "event_properties", properties ?? new Dictionary<string, object>() }
        };

        var payload = new Dictionary<string, object>
        {
            { "api_key", API_KEY },
            { "events", new List<Dictionary<string, object>> { eventObj } }
        };

        // Serialize using Newtonsoft.Json
        string json = JsonConvert.SerializeObject(payload);

        using (UnityWebRequest www = new UnityWebRequest(ENDPOINT, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Amplitude HTTP Error: {www.error}");
                Debug.LogError($"response body: {www.downloadHandler.text}");
            }

            else
                Debug.Log($"Amplitude HTTP Event Sent: {eventName} with properties: {JsonConvert.SerializeObject(properties)}");
        }
    }
}
