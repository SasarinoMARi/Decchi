@echo off

set MSBUILD="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% echo Error: Could not find MSBuild.exe. && goto :eof

del /F /S /Q "Decchi\bin\Release"

%MSBUILD% "%cd%\Decchi\Decchi.csproj" /p:Configuration=Release
