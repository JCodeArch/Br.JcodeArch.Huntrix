#!/usr/bin/env python3
"""Bind actual completed test XML to the clean checkout and content digests."""
import hashlib
import json
import subprocess
import sys
from pathlib import Path

def write(results, project):
    root=Path(results);project=Path(project)
    commit=subprocess.check_output(['git','-C',str(project),'rev-parse','HEAD'],text=True).strip()
    if subprocess.check_output(['git','-C',str(project),'status','--porcelain','--untracked-files=all'],text=True).strip():
        raise ValueError('Source checkout is dirty; use ignored TestResults or an external result directory')
    files={mode:hashlib.sha256((root/(mode+'.xml')).read_bytes()).hexdigest() for mode in ('EditMode','PlayMode')}
    data={'schemaVersion':1,'sourceCommit':commit,'sha256':files}
    (root/'provenance.json').write_text(json.dumps(data,indent=2))
    return data

if __name__=='__main__':
    try:write(sys.argv[1],sys.argv[2])
    except (ValueError,OSError,IndexError,subprocess.CalledProcessError) as exc:
        print('Test provenance rejected: '+str(exc),file=sys.stderr);sys.exit(1)
