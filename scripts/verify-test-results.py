#!/usr/bin/env python3
"""Validate real NUnit results; missing/empty/failed suites fail closed."""
import sys
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
    if any(c.get('result') not in ('Passed', 'Success') for c in cases):
        raise ValueError('NUnit result contains failed, skipped, inconclusive or unexecuted cases')
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
