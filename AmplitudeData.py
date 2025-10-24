import requests
import zipfile
import gzip
import json
import csv
import os
from datetime import datetime

API_KEY = "5e89e99bb579654bfbc0cd077b04c8e9"
SECRET_KEY = "b68a54fa29138170c04706b5d1a1b198"
START = "20251020T00"
END = "20251023T23"
OUTPUT_DIR = "./amplitude_data"
OUTPUT_CSV = f"{OUTPUT_DIR}/all_events.csv"


def download_export_zip():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    url = f"https://amplitude.com/api/2/export?start={START}&end={END}"
    resp = requests.get(url, auth=(API_KEY, SECRET_KEY), stream=True)
    resp.raise_for_status()
    zip_path = os.path.join(OUTPUT_DIR, "export.zip")
    with open(zip_path, "wb") as f:
        for chunk in resp.iter_content(chunk_size=8192):
            f.write(chunk)
    return zip_path


def extract_all_events(zip_path):
    all_events = []
    with zipfile.ZipFile(zip_path, "r") as z:
        for name in z.namelist():
            if not name.endswith(".json.gz"):
                continue
            with z.open(name) as gz:
                with gzip.GzipFile(fileobj=gz) as f:
                    for line in f:
                        try:
                            e = json.loads(line)
                            all_events.append(e)
                        except Exception as err:
                            continue
    return all_events


def write_csv(events):
    if not events:
        return

    keys = set()
    for e in events:
        keys.update(k for k, v in e.items() if not isinstance(v, dict))
        if isinstance(e.get("event_properties"), dict):
            keys.update(f"event_properties.{k}" for k in e["event_properties"])

    keys = sorted(keys)

    with open(OUTPUT_CSV, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=keys)
        writer.writeheader()

        for e in events:
            flat = {k: v for k, v in e.items() if not isinstance(v, dict)}
            if isinstance(e.get("event_properties"), dict):
                for k, v in e["event_properties"].items():
                    flat[f"event_properties.{k}"] = v
            writer.writerow(flat)

def main():
    zip_path = download_export_zip()
    events = extract_all_events(zip_path)
    write_csv(events)

if __name__ == "__main__":
    main()