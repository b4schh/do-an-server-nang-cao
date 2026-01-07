#!/bin/bash

# ========================================
# SCRIPT CHẠY UNIT TESTS LOCAL (Linux/Mac)
# ========================================

set -e

echo "========================================"
echo "🧪 Running Unit Tests"
echo "========================================"
echo ""

# Restore dependencies
echo "[1/3] Restoring dependencies..."
dotnet restore
echo "✅ Dependencies restored"
echo ""

# Build test project
echo "[2/3] Building test project..."
dotnet build src/DoAn.Tests/DoAn.Tests.csproj --no-restore
echo "✅ Test project built"
echo ""

# Run tests
echo "[3/3] Running tests..."
dotnet test src/DoAn.Tests/DoAn.Tests.csproj \
    --no-build \
    --verbosity normal \
    --logger "console;verbosity=detailed"

echo ""
echo "========================================"
echo "✅ ALL TESTS PASSED!"
echo "========================================"
