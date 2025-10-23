using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PortalDotsPlotter : MonoBehaviour
{
    [Header("CSV Settings")]
    public string csvFileName = "portals";
    public string gameLevel = "Level_1";

    [Header("Dot Prefabs")]
    public GameObject bluePortalDotPrefab;
    public GameObject orangePortalDotPrefab;

    private List<GameObject> spawnedDots = new List<GameObject>();

    void Start()
    {
        PlotDots();
    }

    public void PlotDots()
    {
        foreach (var dot in spawnedDots)
            Destroy(dot);
        spawnedDots.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            return;
        }
        var allEvents = CSVReader.ReadCSV(csvFile);

        // Check only shot_blue_portal and shot_orange_portal from the portals.csv with the gameLevel
        var filtered = allEvents.Where(e =>
            e.gameLevel == gameLevel &&
            (e.eventType == "shot_blue_portal" || e.eventType == "shot_orange_portal")
        ).ToList();

        // Create these dots with the appropriate Prefab for the Dots
        foreach (var e in filtered)
        {
            GameObject prefab = e.eventType == "shot_blue_portal"
                ? bluePortalDotPrefab
                : orangePortalDotPrefab;

            var dot = Instantiate(prefab, new Vector3(e.x, e.y, 0f), Quaternion.identity, transform);
            dot.name = $"{e.eventType}_{e.x:F2}_{e.y:F2}";
            spawnedDots.Add(dot);
        }

        Debug.Log($"Total of {spawnedDots.Count} portal dots created for {gameLevel}");
    }
}
