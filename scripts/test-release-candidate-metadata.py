#!/usr/bin/env python3
"""Structural RC metadata tests; synthetic fixtures are never release evidence."""
import hashlib
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('rc', Path(__file__).with_name('check-release-candidate.py'))
rc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(rc)


class AcceptanceMetadataTests(unittest.TestCase):
    def check_metadata(self, field=None, value=None):
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
            # Build artifact validation is covered by the existing tooling suite.
            # Mock only that expensive backend; exercise the real RC gate and XML verifier.
            if field == 'approver':
                manifest[field] = value
                path.write_text(json.dumps(manifest))
            elif field:
                report_path = root / '100.json'
                report = json.loads(report_path.read_text())
                report[field] = value
                report_path.write_text(json.dumps(report))
            with patch.object(rc.build, 'verify', side_effect=lambda path: dict(target=path.name, sourceCommit=commit, buildGuid=guid)):
                return rc.verify(path)

    def test_accepts_existing_complete_metadata_shape(self):
        self.assertEqual(self.check_metadata(), 'a' * 40)

    def test_rejects_blank_or_wrong_type_acceptance_strings(self):
        for field in ('reviewer', 'executedUtc', 'approver'):
            for value in ('', '   ', '\t\n', None, True, 1, ['Reviewer'], {'name': 'Reviewer'}):
                with self.subTest(field=field, value=value):
                    with self.assertRaises(ValueError):
                        self.check_metadata(field, value)

    def test_rejects_missing_blank_or_wrong_type_evidence_references(self):
        for value in (None, [], '', 'reference', True, 1, {'url': 'reference'}, [''], ['   '], ['valid', '\t'], [1], [True], [{}]):
            with self.subTest(value=value):
                with self.assertRaises(ValueError):
                    self.check_metadata('evidence', value)


if __name__ == '__main__':
    unittest.main()
