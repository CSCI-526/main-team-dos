# main-team-dos

# How to get the DotPlotter for Portals

1. Run AmplitudeData.py to get amplitude_data/all_events.csv
2. all_events.csv file has ALL the events from Amplitude in the time period that we wanted
3. Then use FilterEventData.py to get the CSV for the shot_blue_portal and shot_orange_portal events with X,Y,gameLevel values
4. The FilterEventData.py will create a portal_events.csv file
5. Copy portal_events.csv as portals.csv in the Resources folder in Unity
6. There exists an Empty GameObject called DotPlotter in All Levels, Activate it and run it with the proper prefabs
7. The script should plot all the blue and orange portals that were shot
