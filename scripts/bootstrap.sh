#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

OPENCODE_AGENTS_DIR="/root/.config/opencode/agents"
OPENCODE_JSON="/root/.config/opencode/opencode.json"

echo "=== AgentPlatform Bootstrap ==="
echo ""

# ── 1. Sync opencode agents ──────────────────────────────────────────────────
echo "Syncing opencode agents..."
mkdir -p "$OPENCODE_AGENTS_DIR"

for src in "$REPO_ROOT/agents/opencode/"*.md; do
    name="$(basename "$src")"
    dest="$OPENCODE_AGENTS_DIR/$name"
    cp "$src" "$dest"
    echo "  ✓ $name"
done

# ── 2. Set default_agent to master-agent ────────────────────────────────────
echo ""
echo "Setting default_agent → master-agent..."
if command -v jq &>/dev/null; then
    tmp="$(mktemp)"
    jq '.default_agent = "master-agent"' "$OPENCODE_JSON" > "$tmp" && mv "$tmp" "$OPENCODE_JSON"
    echo "  ✓ opencode.json updated"
else
    echo "  ! jq not found — update default_agent manually in $OPENCODE_JSON"
fi

# ── 3. Build TypeScript MCPs ─────────────────────────────────────────────────
echo ""
echo "Building TypeScript MCPs..."

build_mcp() {
    local dir="$1"
    local name="$(basename "$dir")"
    if [[ -f "$dir/package.json" ]]; then
        echo "  Building $name..."
        (cd "$dir" && npm run build 2>&1 | tail -3)
        echo "  ✓ $name built"
    fi
}

build_mcp "$REPO_ROOT/mcps/eng-standards-mcp"
build_mcp "$REPO_ROOT/mcps/aec-scaffold-mcp"
build_mcp "$REPO_ROOT/mcps/azdo-mcp"
build_mcp "$REPO_ROOT/mcps/dotnet-docs-mcp"

# ── 4. Install vs-build ──────────────────────────────────────────────────────
echo ""
echo "Installing vs-build..."
VS_BUILD_SRC="$REPO_ROOT/tools/vs-build/vs-build"
if [[ -f "$VS_BUILD_SRC" ]]; then
    chmod +x "$VS_BUILD_SRC"
    ln -sf "$VS_BUILD_SRC" /usr/local/bin/vs-build
    echo "  ✓ vs-build → /usr/local/bin/vs-build"
else
    echo "  ! vs-build script not found at $VS_BUILD_SRC"
fi

# ── 5. Build dar-cli ─────────────────────────────────────────────────────────
echo ""
echo "Building dar-cli..."
DAR_CLI_DIR="$REPO_ROOT/tools/dar-cli/src/DAR.Cli"
if [[ -f "$DAR_CLI_DIR/DAR.Cli.csproj" ]]; then
    (cd "$DAR_CLI_DIR" && dotnet build -c Release --nologo -v q 2>&1 | tail -5)
    echo "  ✓ dar-cli built"
else
    echo "  ! DAR.Cli.csproj not found at $DAR_CLI_DIR"
fi

echo ""
echo "=== Bootstrap complete ==="
