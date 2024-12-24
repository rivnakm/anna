#!/usr/bin/env bash

set -e

rm -rfv **/TestResults
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage_report
