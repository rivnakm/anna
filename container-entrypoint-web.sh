#!/usr/bin/env bash

python3 set_api_url.py wwwroot/appsettings.json

caddy run --config ./Caddyfile