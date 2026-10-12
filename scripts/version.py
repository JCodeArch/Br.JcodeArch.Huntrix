#!/usr/bin/env python3
"""Validate or bump the repository version; never creates tags or releases."""
import argparse
import re
from pathlib import Path

VERSION_FILE = Path(__file__).resolve().parents[1] / "VERSION"
PATTERN = re.compile(r"(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?\Z")


def parse(value):
    match = PATTERN.fullmatch(value)
    if not match or any(part.isdigit() and len(part) > 1 and part.startswith("0")
                        for part in (match.group(4) or "").split(".")):
        raise ValueError("VERSION must be a valid semantic version")
    return match


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=("check", "major", "minor", "patch", "set"))
    parser.add_argument("value", nargs="?")
    args = parser.parse_args()
    previous = VERSION_FILE.read_text(encoding="utf-8").strip()
    match = parse(previous)
    if args.action == "check":
        if args.value:
            parser.error("check does not accept a value")
        print(previous)
        return
    if args.action == "set":
        if not args.value:
            parser.error("set requires an exact version")
        value = args.value
        parse(value)
    else:
        if args.value:
            parser.error("bump actions do not accept a value")
        parts = [int(match.group(i)) for i in range(1, 4)]
        index = {"major": 0, "minor": 1, "patch": 2}[args.action]
        parts[index] += 1
        for i in range(index + 1, 3):
            parts[i] = 0
        value = ".".join(map(str, parts))
    VERSION_FILE.write_text(value + "\n", encoding="utf-8")
    print(f"{previous} -> {value}")


if __name__ == "__main__":
    main()
