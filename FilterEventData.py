import pandas as pd

INPUT_CSV = "./amplitude_data/all_events.csv"   
OUTPUT_CSV = "./amplitude_data/portal_events.csv"

TARGET_EVENTS = ["shot_blue_portal", "shot_orange_portal"]
COLUMNS_TO_KEEP = [
    "event_type",
    "event_properties.game_level",
    "event_properties.x_position",
    "event_properties.y_position",
]

def main():
    df = pd.read_csv(INPUT_CSV, sep="\t" if "\t" in open(INPUT_CSV).read(1000) else ",", low_memory=False)
    df_filtered = df[df["event_type"].isin(TARGET_EVENTS)]
    existing_cols = [c for c in COLUMNS_TO_KEEP if c in df_filtered.columns]
    df_final = df_filtered[existing_cols]
    df_final.to_csv(OUTPUT_CSV, index=False)

if __name__ == "__main__":
    main()
