@echo off

IF EXIST "%~2ConfuserEx\Confuser.CLI.exe" (
	echo ^<project outputDir="%~3Confused" baseDir="%~3" xmlns="http://confuser.codeplex.com"^>	>  "%~3confuserex.crproj"
	echo ^<rule pattern="true" inherit="false"^>												>> "%~3confuserex.crproj"
	echo ^<protection id="resources"/^>															>> "%~3confuserex.crproj"
	echo ^</rule^>																				>> "%~3confuserex.crproj"
	echo ^<module path="%~4"/^>																	>> "%~3confuserex.crproj"
	echo ^</project^>																			>> "%~3confuserex.crproj"

	call "%~2ConfuserEx\Confuser.CLI.exe" -n "%~3confuserex.crproj"
)
