#!/usr/bin/env bash
set -euo pipefail
: "${UNITY_EDITOR:?Set UNITY_EDITOR to the licensed Unity 6000.6.4f1 executable}"
project_path="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
platform="${1:?Specify android-dev, android-release, ios, windows-dev or windows-release}"
output_path="${2:?Specify a fresh build output path}"
scenes="${3:?Specify explicit scene paths separated by semicolons}"
export HUNTRX_BUILD_NUMBER="${HUNTRX_BUILD_NUMBER:-${GITHUB_RUN_NUMBER:-}}"
source_commit="$(git -C "$project_path" rev-parse HEAD)"
if [ -n "$(git -C "$project_path" status --porcelain --untracked-files=all)" ]; then echo "Build requires a clean source checkout" >&2; exit 2; fi
if [ -n "${GITHUB_SHA:-}" ] && [ "$GITHUB_SHA" != "$source_commit" ]; then echo "GITHUB_SHA differs from checkout" >&2; exit 2; fi
export GITHUB_SHA="$source_commit"
mkdir -p "$(dirname "$output_path")"
case "$platform" in
  android-dev) target=Android; method=AndroidDevelopment ;;
  android-release) target=Android; method=AndroidRelease ;;
  ios) target=iOS; method=IOSProject ;;
  windows-dev) target=StandaloneWindows64; method=WindowsDevelopment ;;
  windows-release) target=StandaloneWindows64; method=WindowsRelease ;;
  *) echo 'Unknown build platform' >&2; exit 2 ;;
esac
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$project_path" -buildTarget "$target" -executeMethod "HuntrX.Editor.Build.PrototypeBuildPipeline.$method" -huntrxOutput "$output_path" -huntrxScenes "$scenes" -huntrxEvidence "$output_path.evidence.json" -logFile "$output_path.unity.log"
if [ "$(git -C "$project_path" rev-parse HEAD)" != "$source_commit" ]; then
  echo "Source HEAD changed during build; artifact acceptance rejected" >&2
  exit 2
fi
python3 "$project_path/scripts/verify-build-evidence.py" "$output_path.evidence.json"
