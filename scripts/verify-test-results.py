#!/usr/bin/env python3
"""Validate real NUnit results; missing/empty/failed suites fail closed."""
import sys
import re
from pathlib import Path
import xml.etree.ElementTree as ET

def verify(path):
    path = Path(path)
    if not path.is_file():
        raise ValueError(f'Missing NUnit result: {path.name}')
    root = ET.parse(path).getroot()
    cases = list(root.iter('test-case'))
    if not cases:
        raise ValueError('No executed test cases in NUnit result')
    nodes = [root, *root.iter('test-suite'), *cases]
    for node in nodes:
        for counter in ('failed', 'failures', 'errors'):
            value = node.get(counter)
            if value is not None and (re.fullmatch(r'[0-9]+', value) is None or int(value) != 0):
                raise ValueError(f'NUnit failure counter {counter} is nonzero or invalid')
    if any(node.get('executed', 'true').lower() not in ('true', '1') for node in nodes):
        raise ValueError('NUnit result contains an explicitly unexecuted node')
    if any(c.get('result') not in ('Passed', 'Success') for c in cases):
        raise ValueError('NUnit result contains failed, skipped, inconclusive or unexecuted cases')
    if any(suite.get('result') not in ('Passed', 'Success') for suite in root.iter('test-suite')):
        raise ValueError('NUnit result contains a nonpassing or unexecuted suite')
    if root.get('result') not in ('Passed', 'Success'):
        raise ValueError('NUnit suite did not pass')
    return len(cases)

if __name__ == '__main__':
    try:
        count = verify(sys.argv[1])
        print(f'Verified {count} real passed NUnit cases')
    except (ValueError, ET.ParseError, OSError, IndexError) as exc:
        print(f'Test evidence rejected: {exc}', file=sys.stderr)
        sys.exit(1)
