@echo off
setlocal enableextensions
title FSH Starter - Detener

echo ============================================================
echo   Deteniendo la aplicacion
echo ============================================================
echo.

REM --- 1) Detener AppHost, API y migrador ---
echo Deteniendo AppHost, API y migrador...
powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -match 'FSH\.Starter\.(AppHost|Api|DbMigrator)' } | ForEach-Object { try { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue } catch {} }"

REM --- 2) Detener los servidores de desarrollo (Vite) de admin y dashboard ---
echo Deteniendo los servidores de desarrollo (dashboard y admin)...
powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'node.exe' -and $_.CommandLine -match 'clients[\\/](admin|dashboard)' } | ForEach-Object { try { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue } catch {} }"

echo Procesos de la app detenidos.
echo.

REM --- 3) Opcional: detener tambien los contenedores Docker ---
choice /c SN /n /m "Detener tambien los contenedores Docker (Postgres/Redis/MinIO)? [S/N]: "
if errorlevel 2 goto fin

echo.
echo Deteniendo contenedores...
for %%n in (postgres redis minio pgadmin redis-insight) do (
    for /f "tokens=*" %%i in ('docker ps -q --filter "name=%%n" 2^>nul') do docker stop %%i >nul 2>&1
)
echo Contenedores detenidos. Los datos se conservan en los volumenes.

:fin
echo.
echo Listo. La aplicacion se ha detenido.
pause
endlocal
