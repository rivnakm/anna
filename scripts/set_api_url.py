import json
import os
import sys

def main():
    appsettings_file = sys.argv[1]
    api_url = os.environ["ANNA_API_URL"]
    
    data = {}
    with open(appsettings_file, "r", encoding="utf-8") as file:
        data = json.load(file)

    data["AnnaClient"]["IndexUrl"] = api_url

    with open(appsettings_file, "w", encoding="utf-8") as file:
        json.dump(data, file, indent=2)
        

if __name__ == "__main__":
    main()