@echo off

set MSBUILD="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% echo Error: Could not find MSBuild.exe. && goto :eof


SET CDIR=%cd%\Decchi\bin\Confused\
SET RDIR=%cd%\Decchi\bin\Release\

del /F /S /Q "Decchi\bin\Release"
del /F /S /Q "Decchi\bin\Confused"

%MSBUILD% "%cd%\Decchi\Decchi.csproj" /p:Configuration=Release

echo ^<project outputDir="%CDIR%" baseDir="%RDIR%" xmlns="http://confuser.codeplex.com"^>	>  "%RDIR%ConfuserEx.crproj"
echo ^<rule pattern="true" inherit="false"^>												>> "%RDIR%ConfuserEx.crproj"
echo ^<protection id="resources"/^>															>> "%RDIR%ConfuserEx.crproj"
echo ^</rule^>																				>> "%RDIR%ConfuserEx.crproj"
echo ^<module path="Decchi.exe"/^>							>> "%RDIR%ConfuserEx.crproj"
echo ^</project^>																			>> "%RDIR%ConfuserEx.crproj"

"%cd%\ConfuserEx\Confuser.CLI.exe" -n "%RDIR%confuserex.crproj"
