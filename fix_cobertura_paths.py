import os
import xml.etree.ElementTree as ET

from pathlib import Path
from typing import List

project_root = Path(os.getcwd())


def find_projects() -> List[Path]:
    return list(project_root.glob("*.Test"))


def coverage_report(project_path: Path) -> Path:
    return next(project_path.glob("./TestResults/*/coverage.cobertura.xml"))


def fix_report(report_path: Path):
    tree = ET.parse(report_path)
    root = tree.getroot()

    sources = root.find("sources")
    assert sources is not None

    source = sources.find("source")
    assert source is not None

    source_path = source.text
    assert source_path is not None

    for file in root.iter("class"):
        current = file.get("filename")
        assert current is not None

        file_path = Path(source_path, current)
        fixed = file_path.relative_to(project_root)

        file.set("filename", str(fixed))

    source.text = str(project_root)

    tree.write(report_path)


def main():
    projects = find_projects()

    for proj in projects:
        report = coverage_report(proj)
        fix_report(report)


if __name__ == "__main__":
    main()
