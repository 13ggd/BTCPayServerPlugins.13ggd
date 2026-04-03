@echo off
REM Script para copiar a DLL com o nome correto para o Plugin Builder

cd /d "bin\Release\net8.0\"

REM Se a DLL com nome longo não existe, mas a simplecashbr.dll existe
if not exist "BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR.dll" (
    if exist "simplecashbr.dll" (
        copy simplecashbr.dll BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR.dll
        echo DLL copiada com nome correto para Plugin Builder
    ) else (
        echo Nenhuma DLL encontrada
    )
) else (
    echo DLL com nome correto ja existe
)

pause
