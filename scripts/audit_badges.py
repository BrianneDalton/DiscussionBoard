#!/usr/bin/env python3
"""Generate portable, repository-local SVG badges from a NuGet audit report."""
import argparse
import json
from pathlib import Path
from xml.sax.saxutils import escape

SEVERITIES = ("critical", "high", "moderate", "low")
COLORS = {"critical": "#b91c1c", "high": "#c2410c", "moderate": "#9a6700", "low": "#996500"}
GREEN = "#18794e"
UNKNOWN = "#666666"


def collect(report):
    projects = report.get("projects")
    if not isinstance(projects, list):
        raise ValueError("NuGet audit report must contain a projects array")
    found = {severity: set() for severity in SEVERITIES}
    for project in projects:
        for framework in project.get("frameworks", []):
            for group in ("topLevelPackages", "transitivePackages"):
                for package in framework.get(group, []):
                    for advisory in package.get("vulnerabilities", []):
                        severity = str(advisory.get("severity", "")).lower()
                        if severity not in found:
                            raise ValueError(f"Unrecognized severity {severity!r}")
                        url = advisory.get("advisoryurl", advisory.get("advisoryUrl"))
                        if not url or not package.get("id") or not package.get("resolvedVersion"):
                            raise ValueError("Advisory is missing package, version or URL")
                        found[severity].add((package["id"].lower(), package["resolvedVersion"], url))
    return {severity: len(found[severity]) for severity in SEVERITIES}


def badge(label, value, color):
    """Static, accessible, self-contained SVG; no external badge service."""
    label_width = max(80, 8 * len(label) + 20)
    value_width = max(48, 8 * len(value) + 20)
    total = label_width + value_width
    label = escape(label)
    value = escape(value)
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="{total}" height="24" role="img" aria-label="{label}: {value}">
<title>{label}: {value}</title>
<rect width="{label_width}" height="24" fill="#404040"/>
<rect x="{label_width}" width="{value_width}" height="24" fill="{color}"/>
<g fill="#fff" font-family="DejaVu Sans,Arial,sans-serif" font-size="12" text-anchor="middle">
<text x="{label_width / 2}" y="16">{label}</text>
<text x="{label_width + value_width / 2}" y="16">{value}</text>
</g></svg>
'''


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--report", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--scan-succeeded", action="store_true")
    args = parser.parse_args()
    counts = None
    if args.scan_succeeded:
        try:
            counts = collect(json.loads(Path(args.report).read_text(encoding="utf-8")))
        except (OSError, ValueError, TypeError) as exc:
            print(f"Audit report invalid: {exc}")
    status = "unknown" if counts is None else (
        "failing" if any(counts[k] for k in ("critical", "high", "moderate")) else "passing"
    )
    output = Path(args.output)
    output.mkdir(parents=True, exist_ok=True)
    status_color = UNKNOWN if status == "unknown" else (GREEN if status == "passing" else COLORS["critical"])
    (output / "audit.svg").write_text(badge("Dependency audit", status, status_color), encoding="utf-8")
    for severity in SEVERITIES:
        count = None if counts is None else counts[severity]
        value = "unknown" if count is None else str(count)
        color = UNKNOWN if count is None else (GREEN if count == 0 else COLORS[severity])
        (output / f"{severity}.svg").write_text(badge(severity.title(), value, color), encoding="utf-8")
    (output / "summary.json").write_text(
        json.dumps({"status": status, "counts": counts}, indent=2) + "\n", encoding="utf-8"
    )
    print(f"Audit status: {status}; counts: {counts}")
    return 0 if counts is not None else 2


if __name__ == "__main__":
    raise SystemExit(main())
