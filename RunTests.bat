@echo off
setlocal enabledelayedexpansion

set "projDirectory"=."
set "testsDirectory=.\FrameworkTests"
set "testLibraryName=*"
set "testName=*"

:parseArguments
if "%~1" neq "" (
    if "%~1"=="--lib" (
        if "%~2" neq "" (
            set "testLibraryName=%~2"
            shift
            shift
        ) else (
            echo [ERROR] No value provided for --lib
            exit /b
        )
    ) else if "%~1"=="--test" (
        if "%~2" neq "" (
            set "testName=%~2"
            shift
            shift
        ) else (
            echo [ERROR] No value provided for --test
            exit /b
        )
    ) else if "%~1"=="--tests-root-dir" (
        if "%~2" neq "" (
            set "testsDirectory=%~2"
            shift
            shift
        ) else (
            echo [ERROR] No value provided for --tests-root-dir
            exit /b
        )
    ) else if "%~1"=="--proj-root-dir" (
    if "%~2" neq "" (
        set "projDirectory=%~2"
        shift
        shift
    ) else (
        echo [ERROR] No value provided for --proj-root-dir
        exit /b
    )
    ) else (
        echo [ERROR] Invalid parameter: %1
        exit /b
    )
    goto :parseArguments
)

if "%testLibraryName%" neq "*" (
    if not exist "%testsDirectory%\%testLibraryName%\" (
        echo [ERROR] The selected test library directory does not exist: "%testsDirectory%\%testLibraryName%\"
        exit /b
    )

    if "%testName%" neq "*" (
        if not exist "%testsDirectory%\%testLibraryName%\%testName%\" (
            echo [ERROR] The selected test directory does not exist: "%testsDirectory%\%testLibraryName%\%testName%\"
            exit /b
        )
        else (
            echo Running test: !testName!
            dotnet run !testName!
            goto :endExecution
        )
    )
)

for /d %%L in ("%testsDirectory%\%testLibraryName%") do (
    set "subdirectory=%%~fL"
    echo Found test library: %%L
    for /d %%T in ("!subdirectory!\%testName%") do (
        set "testName=%%~nxT"
        echo Found test: %%T
        echo Running test: !testName!
        dotnet run --project "%projDirectory%" --no-build -- !testName!
    )
)

:endExecution
color 0
echo Tests execution finished
exit /b
