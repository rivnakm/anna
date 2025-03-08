import json
import os
import sys

def main():
    appsettings_file = sys.argv[1]
    origins = list(filter(lambda o: o != "", os.environ["ANNA_WEB_DOMAINS"].split(";")))
    
    data = {}
    with open(appsettings_file, "r", encoding="utf-8") as file:
        data = json.load(file)

    data["Cors"]["AllowedOrigins"] = origins

    with open(appsettings_file, "w", encoding="utf-8") as file:
        json.dump(data, file, indent=2)
        

if __name__ == "__main__":
    main()