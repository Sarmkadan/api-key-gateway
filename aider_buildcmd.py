#!/usr/bin/env python3
"""
Simple build command script for the task-factory repository.

Running this script will execute `dotnet test` in the repository root,
allowing you to compile and run all unit tests (including the newly
added CollectionExtensionsValidationTests).

Usage:
    python3 /home/redrocket/task-factory/aider_buildcmd.py
"""

import subprocess
import sys
import pathlib

def main() -> None:
    # Determine the repository root (the directory containing this script)
    repo_root = pathlib.Path(__file__).resolve().parent

    # Run `dotnet test` in the repository root
    try:
        result = subprocess.run(
            ["dotnet", "test"],
            cwd=repo_root,
            check=False,
        )
    except FileNotFoundError:
        print("Error: 'dotnet' executable not found. Ensure the .NET SDK is installed and on PATH.", file=sys.stderr)
        sys.exit(1)

    # Propagate the exit code from `dotnet test`
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
