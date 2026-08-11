#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
log="$repo_root/solutions/ai-collaboration-log.md"

if [[ ! -f "$log" ]]; then
  echo "Missing solutions/ai-collaboration-log.md" >&2
  exit 1
fi

entry_count="$(grep -Ec '^## [0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2} — ' "$log" || true)"
if [[ "$entry_count" -eq 0 ]]; then
  echo "AI collaboration log contains no chronological entries." >&2
  exit 1
fi

if ! grep -Eq '^(Accepted|Rejected|Modified)( |$)' "$log"; then
  echo "AI collaboration log has no explicit candidate decision." >&2
  exit 1
fi

echo "AI collaboration log check passed: $entry_count entries."
