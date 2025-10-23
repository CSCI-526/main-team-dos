using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PortalEvent
{
    public string eventType;
    public string gameLevel;
    public float x;
    public float y;
}

public class CSVReader
{
    public static List<PortalEvent> ReadCSV(TextAsset csvFile)
    {
        var events = new List<PortalEvent>();
        using (StringReader reader = new StringReader(csvFile.text))
        {
            bool firstLine = true;
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                if (firstLine)
                {
                    firstLine = false;
                    continue;
                }

                var values = line.Split(',');
                if (values.Length < 4) continue;

                var e = new PortalEvent
                {
                    eventType = values[0].Trim(),
                    gameLevel = values[1].Trim(),
                    x = float.Parse(values[2]),
                    y = float.Parse(values[3])
                };
                events.Add(e);
            }
        }
        return events;
    }
}
