#!/usr/bin/env bash
set -euo pipefail
: "${UNITY_EDITOR:?Set UNITY_EDITOR to the licensed Unity 6000.6.4f1 executable}"
editor_version="$("$UNITY_EDITOR" -version)"
if [ "$editor_version" != 6000.6.4f1 ]; then echo 'Pinned Unity Editor version required' >&2; exit 2; fi
project_path="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
results_root="${1:-$project_path/TestResults}"
results_path="$results_root/$(date -u '+%Y%m%dT%H%M%S')-$$"
mkdir -p "$results_path"
for test_mode in EditMode PlayMode; do
  "$UNITY_EDITOR" -batchmode -nographics -projectPath "$project_path" -runTests -testPlatform "$test_mode" -testResults "$results_path/$test_mode.xml" -logFile "$results_path/$test_mode.log"
  python3 "$project_path/scripts/verify-test-results.py" "$results_path/$test_mode.xml"
done

python3 "$project_path/scripts/write-test-provenance.py" "$results_path" "$project_path"
