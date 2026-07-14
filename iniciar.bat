@echo off
setlocal enableextensions
title FSH Starter - Avicola

REM Ir a la carpeta del repositorio (donde esta este .bat)
cd /d "%~dp0"

echo ============================================================
echo   Iniciando la aplicacion
echo   (Aspire: Postgres + Redis + MinIO + migrador + API + apps)
echo ============================================================
echo.

REM --- 1) Asegurar que Docker Desktop esta corriendo ---
docker ps >nul 2>&1
if not errorlevel 1 goto docker_ok

echo Docker no responde. Iniciando Docker Desktop...
if exist "%ProgramFiles%\Docker\Docker\Docker Desktop.exe" (
    start "" "%ProgramFiles%\Docker\Docker\Docker Desktop.exe"
) else (
    echo [AVISO] No se encontro Docker Desktop. Abrelo manualmente.
)

echo Esperando a que Docker arranque (puede tardar 1-2 minutos)...
set /a _try=0
:wait_docker
set /a _try+=1
timeout /t 6 /nobreak >nul
docker ps >nul 2>&1
if not errorlevel 1 goto docker_ok
if %_try% geq 30 (
    echo.
    echo [ERROR] Docker no estuvo listo a tiempo. Abre Docker Desktop y reintenta.
    pause
    exit /b 1
)
goto wait_docker

:docker_ok
echo Docker OK.
echo.

REM --- 2) Certificado HTTPS de desarrollo (necesario para el dashboard) ---
dotnet dev-certs https --check --trust >nul 2>&1
if errorlevel 1 (
    echo Confirmando el certificado HTTPS de desarrollo...
    dotnet dev-certs https --trust
)

REM --- 3) Levantar el stack ---
REM NuGetAudit=false evita que un aviso de paquete transitivo (NU1903)
REM bloquee la compilacion bajo TreatWarningsAsErrors.
set NuGetAudit=false

echo.
echo Compilando y levantando el stack. La primera vez tarda varios minutos.
echo.
echo   Cuando termine de arrancar:
echo     - Dashboard avicola : http://localhost:5174   (admin@acme.com / Password123!)
echo     - Admin (operador)  : http://localhost:5173
echo     - API + Scalar      : https://localhost:7030/scalar
echo     - Panel de Aspire   : ver la URL "Login to the dashboard" mas abajo
echo.
echo   Deja esta ventana abierta. Pulsa Ctrl+C para detener la app.
echo ============================================================
echo.

dotnet run --project src\Host\FSH.Starter.AppHost

echo.
echo La aplicacion se ha detenido.
pause
endlocal
