#!/bin/bash
# Script para copiar a DLL com o nome correto para o Plugin Builder

cd /build/Plugins/BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR/bin/Release/net8.0/

# Se a DLL com nome longo não existe, mas a simplecashbr.dll existe
if [ ! -f "BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR.dll" ] && [ -f "simplecashbr.dll" ]; then
    cp simplecashbr.dll BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR.dll
    echo "DLL copiada com nome correto para Plugin Builder"
elif [ -f "BTCPayServer.Plugins.13ggd.Plugins.SimpleCashBR.dll" ]; then
    echo "DLL com nome correto já existe"
else
    echo "Nenhuma DLL encontrada"
fi
