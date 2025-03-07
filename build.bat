@echo off
set SOLUTION_NAME=VRCLogTail
set SOLUTION_FILE=%SOLUTION_NAME%.sln
set BUILD_CONFIG=Release
set BUILD_PLATFORM="Any CPU"

SET oldcd=%CD%
cd %~dp0

dotnet restore /p:Platform=%BUILD_PLATFORM% %SOLUTION_FILE%
dotnet build -c %BUILD_CONFIG% /p:Platform=%BUILD_PLATFORM% %SOLUTION_FILE%

cd %oldcd%
