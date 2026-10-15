#!/usr/bin/env bash
set -euo pipefail
: "${UNITY_EDITOR:?Set UNITY_EDITOR to the licensed Unity 6000.6.4f1 executable}"
editor_version="$("$UNITY_EDITOR" -version)"
if [ "$editor_version" != 6000.6.4f1 ]; then echo 'Pinned Unity Editor version required' >&2; exit 2; fi
project_path="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
results_root="${1:-$project_path/TestResults}"
results_path="$results_root/$(date -u '+%Y%m%dT%H%M%S')-$$"
mkdir -p "$results_path"
# Freeze identity before either suite. Never discard generated import/settings changes.
source_commit="$(git -C "$project_path" rev-parse HEAD)"
record_failure() {
  result=$?
  if [ "$result" -ne 0 ]; then
    git -C "$project_path" status --porcelain --untracked-files=all > "$results_path/source-status.txt" || true
    git -C "$project_path" diff --binary HEAD > "$results_path/source-changes.patch" || true
    git -C "$project_path" rev-parse HEAD > "$results_path/observed-head.txt" || true
  fi
}
trap record_failure EXIT
printf '%s\n' "$source_commit" > "$results_path/initial-head.txt"
for test_mode in EditMode PlayMode; do
  python3 "$project_path/scripts/write-test-provenance.py" --check-source "$project_path" "$source_commit"
  "$UNITY_EDITOR" -batchmode -nographics -projectPath "$project_path" -runTests -testPlatform "$test_mode" -testResults "$results_path/$test_mode.xml" -logFile "$results_path/$test_mode.log"
  python3 "$project_path/scripts/write-test-provenance.py" --check-source "$project_path" "$source_commit"
  python3 "$project_path/scripts/verify-test-results.py" "$results_path/$test_mode.xml"
done

python3 "$project_path/scripts/write-test-provenance.py" "$results_path" "$project_path" "$source_commit"
