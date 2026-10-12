#!/usr/bin/env python3
"""Fail-closed RC evidence gate. Does not create builds, publish or approve QA."""
import hashlib
import xml.etree.ElementTree as ET
import argparse
import importlib.util
import json
from pathlib import Path

def load_module(name, file):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).parent/file)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value

build=load_module('build','verify-build-evidence.py')
tests=load_module('tests','verify-test-results.py')

def verify(manifest):
    path=Path(manifest).resolve();data=json.loads(path.read_text());base=path.parent
    def artifact(value):
        if not isinstance(value,str) or not value.strip():raise ValueError('Missing evidence path')
        return (base/value).resolve()
    if data.get('schemaVersion')!=1:raise ValueError('Unsupported RC evidence schema')
    commit=data.get('sourceCommit')
    provenance=json.loads(artifact(data['testProvenance']).read_text())
    if provenance.get('schemaVersion')!=1 or provenance.get('sourceCommit')!=commit:raise ValueError('Test provenance does not match RC commit')
    for mode in ('EditMode','PlayMode'):
        xml=artifact(data['tests'][mode]);tests.verify(xml)
        if hashlib.sha256(xml.read_bytes()).hexdigest()!=provenance['sha256'][mode]:raise ValueError('Test XML differs from provenance digest')
    targets=set();build_guids=set()
    for entry in data['builds']:
        evidence=build.verify(artifact(entry))
        if evidence['sourceCommit']!=commit:raise ValueError('Build source commits differ')
        targets.add(evidence['target']);build_guids.add(evidence.get('buildGuid'))
    if targets!={'Android','iOS','StandaloneWindows64'}:raise ValueError('All target artifacts/exports required')
    hardware=set()
    for tier in ('Entry','Standard','High'):
        report=json.loads(artifact(data['performance'][tier]).read_text())
        for field in ('deviceModel','operatingSystem','processorType','graphicsDeviceName','scene','scenario'):
            if not isinstance(report.get(field),str) or not report[field].strip():raise ValueError('Device/scenario metadata is incomplete')
        if any(type(report.get(field)) is not int or report[field]<=0 for field in ('systemMemoryMegabytes','processorCount')):raise ValueError('Device hardware metadata is unavailable')
        identity=(report['deviceModel'],report['processorType'],report['graphicsDeviceName'],report['systemMemoryMegabytes'])
        if identity in hardware:raise ValueError('Different device tiers require representative hardware, not relabeled same-device captures')
        hardware.add(identity)
        if data['deviceTierAssignments'][tier]!=report['deviceModel']:raise ValueError('Device capture does not match reviewed tier assignment')
        if not report.get('buildGuid') or report['buildGuid'] not in build_guids:raise ValueError('Performance capture is from an unrelated build')
        if type(report.get('samples')) is not int or not (600<=report['samples']<=3600):raise ValueError('Insufficient captured device frames')
        for metric,limit in (('frameP95Milliseconds',16.67),('cpuP95Milliseconds',10.0),('gpuP95Milliseconds',12.0)):
            value=report.get(metric)
            if type(value) not in (int,float) or not (0<value<=limit):raise ValueError('Measured percentile does not satisfy prototype budget')
        if type(report.get('peakMemoryBytes')) is not int or not (0<report['peakMemoryBytes']<=536870912):raise ValueError('Measured memory budget is exceeded or unavailable')
        if report.get('tier')!=tier or report.get('isEditor') is not False or report.get('measured') is not True or report.get('withinBudget') is not True:raise ValueError('Missing measured device performance acceptance')
    for card in ('100','101','102','103','104'):
        report=json.loads(artifact(data['qa'][card]).read_text())
        if report.get('status')!='Passed' or report.get('sourceCommit')!=commit or not report.get('reviewer') or not report.get('executedUtc') or not report.get('evidence'):raise ValueError('QA acceptance is pending or incomplete')
    if data.get('releaseApproval')!='Approved' or not data.get('approver'):raise ValueError('Human release approval required')
    return commit

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('manifest');args=parser.parse_args()
    try:print('RC evidence gate satisfied for '+verify(args.manifest))
    except (ValueError,OSError,KeyError,TypeError,ET.ParseError) as exc:parser.exit(1,'RC blocked: '+str(exc)+'\n')
