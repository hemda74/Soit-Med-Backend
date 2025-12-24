#!/bin/bash

echo "Testing SQL Server Connection..."
echo "=================================="
echo ""
echo "Connection String:"
echo "Server=10.10.9.104\\SQLEXPRESS;Database=ITIWebApi44;User Id=soitmed;Password=***"
echo ""
echo "Testing network connectivity to SQL Server..."
echo ""

# Test ping
echo "1. Testing ping to 10.10.9.104..."
if ping -c 2 10.10.9.104 > /dev/null 2>&1; then
    echo "   ✅ Ping successful - Server is reachable"
else
    echo "   ❌ Ping failed - Server is not reachable"
    exit 1
fi

# Test SQL Server Browser port (UDP 1434)
echo ""
echo "2. Testing SQL Server Browser port (UDP 1434)..."
if nc -uv -w 2 10.10.9.104 1434 > /dev/null 2>&1; then
    echo "   ✅ SQL Server Browser port is accessible"
else
    echo "   ⚠️  SQL Server Browser port may not be accessible (this is OK if using specific port)"
fi

# Test common SQL Server ports
echo ""
echo "3. Testing common SQL Server ports..."
for port in 1433 49152 49153 49154; do
    if nc -zv -w 2 10.10.9.104 $port > /dev/null 2>&1; then
        echo "   ✅ Port $port is open"
    fi
done

echo ""
echo "=================================="
echo "Connection Test Complete"
echo ""
echo "To install .NET 8.0 runtime, run:"
echo "  brew install --cask dotnet-sdk@8"
echo ""
echo "Or download directly from:"
echo "  https://dotnet.microsoft.com/download/dotnet/8.0"
echo ""

