#!/usr/bin/env bash
# update-source.sh — regenerate Assets/source.zip from the current src/ tree.
#
# Run this before committing whenever you change source files, so that
# `dar brand` always ships fresh source.
#
# Usage:  ./scripts/update-source.sh
#         bash scripts/update-source.sh   (from repo root)

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$REPO_ROOT/src/DAR.Cli/Assets/source.zip"

echo "[update-source] Regenerating $OUT ..."

python3 - "$REPO_ROOT" "$OUT" << 'PYEOF'
import os, zipfile, sys

root = sys.argv[1]
out  = sys.argv[2]

with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED) as z:
    for dirpath, dirnames, filenames in os.walk(os.path.join(root, 'src')):
        dirnames[:] = [d for d in dirnames if d not in ('bin', 'obj')]
        for fn in filenames:
            if fn == 'source.zip':
                continue
            fp      = os.path.join(dirpath, fn)
            arcname = 'src/' + os.path.relpath(fp, os.path.join(root, 'src')).replace(os.sep, '/')
            z.write(fp, arcname)

size = os.path.getsize(out)
print(f'[update-source] Done — {out} ({size:,} bytes)')
PYEOF
