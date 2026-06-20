import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ANIM_PREFAB_DIR = ROOT / "Assets/Graphics/FattyPolyTurretPart2/Prefabs/Anim"
CLIP_DIR = ROOT / "Assets/Graphics/FattyPolyTurretPart2/Animations"

TOP_Y = 0.4
FIREARM_Y = 1.7
HEAD_OFFSET = 1.7


def fix_prefab(path: Path) -> bool:
    lines = path.read_text(encoding="utf-8").splitlines(keepends=True)
    changed = False
    i = 0
    while i < len(lines):
        name = lines[i].strip()
        if name in ("m_Name: Top", "m_Name: Firearm"):
            target_y = TOP_Y if name.endswith("Top") else FIREARM_Y
            j = i + 1
            while j < len(lines) and "m_LocalPosition:" not in lines[j]:
                j += 1
            if j < len(lines):
                new_line, n = re.subn(
                    r"m_LocalPosition: \{x: 0, y: [-\d.]+, z: 0\}",
                    f"m_LocalPosition: {{x: 0, y: {target_y}, z: 0}}",
                    lines[j],
                    count=1,
                )
                if n:
                    lines[j] = new_line
                    changed = True
        i += 1

    if changed:
        path.write_text("".join(lines), encoding="utf-8", newline="")
    return changed


def fix_clip(path: Path) -> bool:
    lines = path.read_text(encoding="utf-8").splitlines(keepends=True)
    changed = False

    for i, line in enumerate(lines):
        if line.strip() != "path: Turret/Top/Firearm":
            continue

        start = max(0, i - 100)
        in_y_attr = False
        for k in range(i - 1, start - 1, -1):
            stripped = lines[k].strip()
            if stripped == "path: Turret/Top/Firearm":
                break
            if stripped == "attribute: m_LocalPosition.y":
                in_y_attr = True
                continue
            if stripped.startswith("attribute:") and in_y_attr:
                break

            if in_y_attr:
                match = re.match(r"(\s+value: )([-\d.]+)\s*$", lines[k])
                if match and k > 0 and "time:" in lines[k - 1]:
                    new_value = float(match.group(2)) + HEAD_OFFSET
                    lines[k] = f"{match.group(1)}{new_value}\n"
                    changed = True
                continue

            match = re.match(
                r"(\s+value: \{x: )([^,]+)(, y: )([-\d.]+)(, z: )([^}]+)(\})\s*$",
                lines[k],
            )
            if match:
                new_y = float(match.group(4)) + HEAD_OFFSET
                lines[k] = (
                    f"{match.group(1)}{match.group(2)}{match.group(3)}{new_y}"
                    f"{match.group(5)}{match.group(6)}{match.group(7)}\n"
                )
                changed = True

    if changed:
        path.write_text("".join(lines), encoding="utf-8", newline="")
    return changed


def main() -> None:
    prefab_changed = 0
    for path in sorted(ANIM_PREFAB_DIR.glob("FattyMissile*_Anim.prefab")):
        if fix_prefab(path):
            prefab_changed += 1
            print(f"prefab: {path.name}")

    clip_changed = 0
    for name in [
        "Missile_Idle.anim",
        "Missile_Fire.anim",
        "Missile_Reload.anim",
        "Missile_Install.anim",
        "Missile_Remove.anim",
    ]:
        path = CLIP_DIR / name
        if path.exists() and fix_clip(path):
            clip_changed += 1
            print(f"clip: {name}")

    print(f"Done prefabs={prefab_changed} clips={clip_changed}")


if __name__ == "__main__":
    main()
