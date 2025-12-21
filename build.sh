#!/usr/bin/env bash
set -euo pipefail

# LionFire.AgUi.Blazor Build Script
# Usage: ./build.sh [command]
# Commands: restore, build, test, pack, clean, all (default)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="${CONFIGURATION:-Release}"
ARTIFACTS_DIR="${SCRIPT_DIR}/artifacts"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

restore() {
    log_info "Restoring dependencies..."
    dotnet restore "${SCRIPT_DIR}"
}

build() {
    log_info "Building solution (${CONFIGURATION})..."
    dotnet build "${SCRIPT_DIR}" --no-restore --configuration "${CONFIGURATION}"
}

test_() {
    log_info "Running tests..."
    dotnet test "${SCRIPT_DIR}" --no-build --configuration "${CONFIGURATION}" \
        --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --logger trx \
        --results-directory "${SCRIPT_DIR}/TestResults"
}

pack() {
    log_info "Creating NuGet packages..."
    mkdir -p "${ARTIFACTS_DIR}"
    dotnet pack "${SCRIPT_DIR}" --no-build --configuration "${CONFIGURATION}" \
        --output "${ARTIFACTS_DIR}"
    log_info "Packages created in ${ARTIFACTS_DIR}"
    ls -la "${ARTIFACTS_DIR}"/*.nupkg 2>/dev/null || log_warn "No packages found"
}

clean() {
    log_info "Cleaning solution..."
    dotnet clean "${SCRIPT_DIR}" --configuration "${CONFIGURATION}"
    rm -rf "${ARTIFACTS_DIR}"
    rm -rf "${SCRIPT_DIR}/TestResults"
    log_info "Clean complete"
}

format_check() {
    log_info "Checking code formatting..."
    dotnet format "${SCRIPT_DIR}" --verify-no-changes --verbosity diagnostic || {
        log_error "Code formatting issues found. Run 'dotnet format' to fix."
        return 1
    }
}

format_fix() {
    log_info "Fixing code formatting..."
    dotnet format "${SCRIPT_DIR}"
    log_info "Formatting complete"
}

all() {
    restore
    build
    test_
    pack
}

show_help() {
    echo "LionFire.AgUi.Blazor Build Script"
    echo ""
    echo "Usage: ./build.sh [command]"
    echo ""
    echo "Commands:"
    echo "  restore      Restore NuGet dependencies"
    echo "  build        Build the solution"
    echo "  test         Run all tests"
    echo "  pack         Create NuGet packages"
    echo "  clean        Clean build outputs"
    echo "  format-check Check code formatting"
    echo "  format-fix   Fix code formatting"
    echo "  all          Run restore, build, test, and pack (default)"
    echo "  help         Show this help message"
    echo ""
    echo "Environment Variables:"
    echo "  CONFIGURATION  Build configuration (default: Release)"
}

# Main entry point
case "${1:-all}" in
    restore)
        restore
        ;;
    build)
        restore
        build
        ;;
    test)
        restore
        build
        test_
        ;;
    pack)
        restore
        build
        test_
        pack
        ;;
    clean)
        clean
        ;;
    format-check)
        format_check
        ;;
    format-fix)
        format_fix
        ;;
    all)
        all
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        log_error "Unknown command: $1"
        show_help
        exit 1
        ;;
esac

log_info "Done!"
