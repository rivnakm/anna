#!/usr/bin/env bash

CONTAINER_IMAGE="anna:latest"

podman build -t "${CONTAINER_IMAGE}" .
CONTAINER_NAME=$(podman run --rm --detach --publish 8080:8080 "${CONTAINER_IMAGE}")

ANNA_API_HOST="http://localhost:8080" dotnet test

podman logs $CONTAINER_NAME > container.log

podman stop $CONTAINER_NAME
