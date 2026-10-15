#!/usr/bin/env python3
"""Bind actual completed test XML to the clean checkout and content digests."""
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

def check_source(project, expected_commit):
    project=Path(project)
    if not re.fullmatch(r"[0-9a-f]{40}", expected_commit):
        raise ValueError("Expected initial source commit must be a full Git SHA")
    commit=subprocess.check_output(['git','-C',str(project),'rev-parse','HEAD'],text=True).strip()
    if commit != expected_commit:
        raise ValueError('Source HEAD changed since test run started')
    if subprocess.check_output(['git','-C',str(project),'status','--porcelain','--untracked-files=all'],text=True).strip():
        raise ValueError('Source checkout is dirty; use ignored TestResults or an external result directory')
    return commit

def write(results, project, expected_commit):
    root=Path(results)
    commit=check_source(project, expected_commit)
    files={mode:hashlib.sha256((root/(mode+'.xml')).read_bytes()).hexdigest() for mode in ('EditMode','PlayMode')}
    data={'schemaVersion':1,'sourceCommit':commit,'sha256':files}
    (root/'provenance.json').write_text(json.dumps(data,indent=2))
    return data

if __name__=='__main__':
    try:
        if sys.argv[1] == '--check-source':
            check_source(sys.argv[2], sys.argv[3])
        else:
            write(sys.argv[1],sys.argv[2],sys.argv[3])
    except (ValueError,OSError,IndexError,subprocess.CalledProcessError) as exc:
        print('Test provenance rejected: '+str(exc),file=sys.stderr);sys.exit(1)
