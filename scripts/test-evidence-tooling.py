#!/usr/bin/env python3
"""Tooling regression tests; fixtures are synthetic and are never release evidence."""
import importlib.util
import hashlib
import json
import tempfile
import subprocess
import shutil
import os
import unittest
from unittest.mock import patch
from pathlib import Path

def module(name, filename):
    spec = importlib.util.spec_from_file_location(name, Path(__file__).parent / filename)
    value = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(value)
    return value

build = module('build_evidence', 'verify-build-evidence.py')
nunit = module('test_evidence', 'verify-test-results.py')
provenance = module('test_provenance', 'write-test-provenance.py')
rc = module('rc_tooling', 'check-release-candidate.py')

class EvidenceTests(unittest.TestCase):
    def test_missing_output_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'fixture.json'
            path.write_text(json.dumps(dict(editorVersion='6000.6.4f1', result='Succeeded', errors=0, target='Android', completedUtc='2026-10-10T00:00:00+00:00', outputPath='', artifactRoot='')))
            with self.assertRaises(ValueError): build.verify(path)

    def test_empty_and_skipped_suites_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'fixture.xml'
            for xml in ['<test-run result="Passed"/>', '<test-run result="Passed"><test-case result="Skipped"/></test-run>']:
                path.write_text(xml)
                with self.assertRaises(ValueError): nunit.verify(path)

    def test_hash_detects_artifact_mutation(self):
        with tempfile.TemporaryDirectory() as directory:
            artifact = Path(directory) / 'fixture.apk'
            artifact.write_bytes(b'tooling fixture')
            previous = build.artifact_hash(artifact)
            artifact.write_bytes(b'changed tooling fixture')
            self.assertNotEqual(previous, build.artifact_hash(artifact))

    def test_unrelated_artifact_root_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); output = root / 'fixture.apk'; output.write_bytes(b'fixture')
            evidence = dict(editorVersion='6000.6.4f1', result='Succeeded', errors=0, target='Android', completedUtc='2026-10-10T00:00:00+00:00', outputPath=str(output), artifactRoot=str(root))
            path = root / 'fixture.json'; path.write_text(json.dumps(evidence))
            with self.assertRaises(ValueError): build.verify(path)

    def test_test_provenance_records_xml_digests_and_rejects_dirty_source(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for mode in ('EditMode', 'PlayMode'):
                (root / (mode + '.xml')).write_text('<test-run result="Passed"><test-case result="Passed"/></test-run>')
            with patch.object(provenance.subprocess, 'check_output', side_effect=['a' * 40, '']):
                data = provenance.write(root, root, 'a' * 40)
            self.assertEqual(data['sourceCommit'], 'a' * 40)
            self.assertEqual(set(data['sha256']), {'EditMode', 'PlayMode'})
            with patch.object(provenance.subprocess, 'check_output', side_effect=['a' * 40, ' M source.cs']):
                with self.assertRaises(ValueError): provenance.write(root, root, 'a' * 40)

    def test_rc_rejects_boolean_measurements_even_when_summary_claims_passed(self):
        # Only verifier behavior is exercised; build/test backends are explicitly mocked.
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory); commit = 'a' * 40; guid = 'b' * 32
            xml = '<test-run result="Passed"><test-case result="Passed"/></test-run>'
            hashes = {}
            for mode in ('EditMode', 'PlayMode'):
                (root / (mode + '.xml')).write_text(xml)
                hashes[mode] = hashlib.sha256(xml.encode()).hexdigest()
            (root / 'provenance.json').write_text(json.dumps(dict(schemaVersion=1, sourceCommit=commit, sha256=hashes)))
            for tier in ('Entry', 'Standard', 'High'):
                report = dict(tier=tier, deviceModel=tier, operatingSystem='Tooling fixture OS', processorType='Fixture CPU',
                              graphicsDeviceName='Fixture GPU', scene='Assets/Scenes/VerticalSlice.unity', scenario='Fixture',
                              systemMemoryMegabytes=2048, processorCount=4, buildGuid=guid, samples=600,
                              frameP95Milliseconds=10, cpuP95Milliseconds=5, gpuP95Milliseconds=5,
                              peakMemoryBytes=1000000, isEditor=False, measured=True, withinBudget=True)
                (root / (tier + '.json')).write_text(json.dumps(report))
            for card in range(100, 105):
                (root / (str(card) + '.json')).write_text(json.dumps(dict(status='Passed', sourceCommit=commit, reviewer='Tooling fixture', executedUtc='2026-10-10T00:00:00Z', evidence=['Fixture only'])))
            manifest = dict(schemaVersion=1, sourceCommit=commit, testProvenance='provenance.json',
                            tests={mode: mode + '.xml' for mode in ('EditMode', 'PlayMode')},
                            builds=['Android', 'iOS', 'StandaloneWindows64'],
                            performance={tier: tier + '.json' for tier in ('Entry', 'Standard', 'High')},
                            deviceTierAssignments={tier: tier for tier in ('Entry', 'Standard', 'High')},
                            qa={str(card): str(card) + '.json' for card in range(100, 105)}, releaseApproval='Approved', approver='Tooling fixture')
            path = root / 'manifest.json'; path.write_text(json.dumps(manifest))
            with patch.object(rc.build, 'verify', side_effect=lambda path: dict(target=path.name, sourceCommit=commit, buildGuid=guid)):
                self.assertEqual(rc.verify(path), commit)
                report = json.loads((root / 'Entry.json').read_text()); report['cpuP95Milliseconds'] = True
                (root / 'Entry.json').write_text(json.dumps(report))
                with self.assertRaises(ValueError): rc.verify(path)

    def test_provenance_rejects_clean_different_head(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for mode in ('EditMode', 'PlayMode'):
                (root / (mode + '.xml')).write_text('synthetic fixture')
            with patch.object(provenance.subprocess, 'check_output', return_value='b' * 40):
                with self.assertRaisesRegex(ValueError, 'HEAD changed'):
                    provenance.write(root, root, 'a' * 40)
            self.assertFalse((root / 'provenance.json').exists())

    def test_runner_rejects_source_changes_and_preserves_diagnostics(self):
        # A fake Editor emits synthetic XML only; this is a runner regression, not Unity evidence.
        for change in ('commit', 'dirty', 'none', 'initial-dirty'):
            with self.subTest(change=change), tempfile.TemporaryDirectory() as directory:
                root = Path(directory); project = root / 'project'; project.mkdir()
                shutil.copytree(Path(__file__).parent, project / 'scripts', ignore=shutil.ignore_patterns('__pycache__'))
                def git(*args):
                    return subprocess.check_output(['git', '-C', str(project), *args], text=True).strip()
                git('init', '-q'); git('config', 'user.name', 'Tooling fixture'); git('config', 'user.email', 'fixture@example.invalid')
                source = project / 'source.txt'; source.write_text('original')
                git('add', '.'); git('commit', '-qm', 'fixture')
                initial = git('rev-parse', 'HEAD')
                if change == 'initial-dirty': source.write_text('already dirty')
                editor = root / 'fake-editor'
                editor.write_text("""#!/usr/bin/env python3
import sys, pathlib, subprocess, os
args=sys.argv[1:]
if args == ['-version']: print('6000.6.4f1'); sys.exit(0)
def arg(name): return args[args.index(name)+1]
project=pathlib.Path(arg('-projectPath'))
pathlib.Path(arg('-testResults')).write_text('<test-run result="Passed"><test-case result="Passed"/></test-run>')
pathlib.Path(arg('-logFile')).write_text('Synthetic Editor fixture')
if arg('-testPlatform') == 'EditMode' and os.environ['FIXTURE_CHANGE'] in ('dirty','commit'):
    (project/'source.txt').write_text('changed during suite')
    if os.environ['FIXTURE_CHANGE'] == 'commit':
        subprocess.run(['git','-C',str(project),'commit','-qam','changed'],check=True)
""")
                editor.chmod(0o755)
                results = root / 'results'
                run = subprocess.run(['bash', str(project / 'scripts/run-unity-tests.sh'), str(results)],
                    env=dict(os.environ, UNITY_EDITOR=str(editor), FIXTURE_CHANGE=change), capture_output=True, text=True)
                if change == 'none':
                    self.assertEqual(run.returncode, 0, run.stderr)
                    data = json.loads(next(results.glob('*/provenance.json')).read_text())
                    self.assertEqual(data['sourceCommit'], initial)
                else:
                    self.assertNotEqual(run.returncode, 0, 'Source change must reject acceptance')
                    self.assertFalse(list(results.glob('*/provenance.json')))
                    if change != 'initial-dirty':
                        self.assertTrue(list(results.glob('*/EditMode.xml')))
                        self.assertFalse(list(results.glob('*/PlayMode.xml')), 'Stop before running against changed source')
                        self.assertTrue(list(results.glob('*/source-status.txt')))
                        self.assertTrue(list(results.glob('*/source-changes.patch')))
                        self.assertIn('changed during suite', source.read_text())

    def test_malformed_json_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'fixture.json'; path.write_text('{')
            with self.assertRaises(ValueError): build.verify(path)

if __name__ == '__main__': unittest.main()
