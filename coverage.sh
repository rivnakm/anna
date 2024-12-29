#!/usr/bin/env bash

set -e

rm -rfv **/TestResults
dotnet test --collect:"XPlat Code Coverage" --settings unit_tests.runsettings
dotnet reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage_report
