#!/usr/bin/env python3
"""Verify real build report and artifact digest; never treat the cwd as output."""
from datetime import datetime, timezone, timedelta
import hashlib
import json
import re
import sys
from pathlib import Path

def artifact_hash(root):
    root = Path(root)
    directory = root.is_dir()
    files = sorted(root.rglob('*')) if directory else [root]
    manifest = bytearray()
    for path in files:
        if not path.is_file() or path.name.endswith(('.unity.log', '.evidence.json')):
            continue
        name = path.relative_to(root).as_posix() if directory else path.name
        digest = hashlib.sha256()
        with path.open('rb') as stream:
            for chunk in iter(lambda: stream.read(1048576), b''):
                digest.update(chunk)
        manifest.extend(name.encode('utf-8') + b'\0' + digest.hexdigest().encode('ascii') + b'\n')
    if not manifest:
        raise ValueError('No artifact files in manifest')
    return hashlib.sha256(manifest).hexdigest()

def verify(path):
    evidence = json.loads(Path(path).read_text())
    if not isinstance(evidence, dict):
        raise ValueError('Build evidence must be an object')
    if evidence.get('editorVersion') != '6000.6.4f1' or evidence.get('result') != 'Succeeded' or type(evidence.get('errors')) is not int or evidence['errors'] != 0:
        raise ValueError('Build report is missing, failed or uses an unpinned Editor')
    completed = evidence.get('completedUtc')
    if not isinstance(completed, str):
        raise ValueError('Build completion timestamp is required')
    captured = datetime.fromisoformat(completed.replace('Z', '+00:00'))
    if captured.tzinfo is None or captured > datetime.now(timezone.utc) + timedelta(minutes=5):
        raise ValueError('Build completion timestamp must be valid and not in the future')
    targets = {'Android', 'iOS', 'StandaloneWindows64'}
    if evidence.get('target') not in targets:
        raise ValueError('Unknown build target')
    for field in ('outputPath', 'artifactRoot'):
        if not isinstance(evidence.get(field), str) or not evidence[field].strip():
            raise ValueError(f'Missing {field}')
    output = Path(evidence['outputPath'])
    root = Path(evidence['artifactRoot'])
    expected_root = output.parent if evidence['target'] == 'StandaloneWindows64' else output
    if root.resolve() != expected_root.resolve():
        raise ValueError('Artifact root does not match the target output')
    if not output.exists() or (output.is_dir() and not any(output.iterdir())) or (output.is_file() and output.stat().st_size == 0):
        raise ValueError('Build output is missing or empty')
    if evidence['target'] == 'Android' and output.suffix not in ('.apk', '.aab'):
        raise ValueError('Android output must be an APK or AAB')
    if evidence['target'] == 'StandaloneWindows64' and (output.suffix != '.exe' or not (output.parent / (output.stem + '_Data')).is_dir()):
        raise ValueError('Windows output requires an EXE and matching Data directory')
    if evidence['target'] == 'iOS' and (not output.is_dir() or not list(output.glob('*.xcodeproj'))):
        raise ValueError('iOS export requires a generated Xcode project')
    if type(evidence.get('totalBytes')) is not int or evidence['totalBytes'] <= 0:
        raise ValueError('Build report contains no produced bytes')
    if not re.fullmatch(r'[0-9a-f]{64}', evidence.get('artifactSha256', '')) or artifact_hash(root) != evidence['artifactSha256']:
        raise ValueError('Artifact digest does not match the build report')
    if not re.fullmatch(r'[0-9a-f]{32}', evidence.get('buildGuid', '')):
        raise ValueError('Actual Unity build GUID must be recorded')
    if not re.fullmatch(r'[0-9a-f]{40}', evidence.get('sourceCommit', '')):
        raise ValueError('Source commit must be recorded')
    if not isinstance(evidence.get('scenes'), list) or not evidence['scenes']:
        raise ValueError('Explicit build scene list is required')
    if any(not isinstance(scene, str) or not scene.startswith('Assets/Scenes/') or '..' in scene or not scene.endswith('.unity') for scene in evidence['scenes']):
        raise ValueError('Build scenes must be explicit project scene paths')
    return evidence

if __name__ == '__main__':
    try:
        evidence = verify(sys.argv[1])
        print(f"Verified actual {evidence['target']} artifact digest and build report")
    except (ValueError, OSError, KeyError, IndexError, TypeError) as exc:
        print(f'Build evidence rejected: {exc}', file=sys.stderr)
        sys.exit(1)
