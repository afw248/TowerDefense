$ErrorActionPreference = "Stop"

$animFolder = Join-Path $PSScriptRoot "..\Assets\Graphics\FattyPolyTurretPart2\Prefabs\Anim"
$clipFolder = Join-Path $PSScriptRoot "..\Assets\Graphics\FattyPolyTurretPart2\Animations"

function Fix-TopAndFirearmTransforms {
    param([string]$Path)

    $content = Get-Content $Path -Raw
    $original = $content

    $content = [regex]::Replace(
        $content,
        '(?s)(m_Name: Top\r?\n.*?m_LocalPosition: \{x: 0, y: )0(\.0)?(, z: 0\})',
        '${1}0.4${3}'
    )

    $content = [regex]::Replace(
        $content,
        '(?s)(m_Name: Firearm\r?\n.*?m_LocalPosition: \{x: 0, y: )[-\d.]+(, z: 0\})',
        '${1}1.7${2}'
    )

    if ($content -ne $original) {
        Set-Content -Path $Path -Value $content -NoNewline
        return $true
    }

    return $false
}

function Fix-FirearmAnimationY {
    param([string]$Path)

    $content = Get-Content $Path -Raw
    $original = $content

    $content = [regex]::Replace(
        $content,
        '(?s)(attribute: m_LocalPosition\.y\r?\n\s+path: Turret/Top/Firearm\r?\n.*?value: )1\.4',
        '${1}0'
    )

    if ($content -ne $original) {
        Set-Content -Path $Path -Value $content -NoNewline
        return $true
    }

    return $false
}

$prefabCount = 0
Get-ChildItem $animFolder -Filter "FattyMissile*_Anim.prefab" | ForEach-Object {
    if (Fix-TopAndFirearmTransforms -Path $_.FullName) {
        $prefabCount++
        Write-Host "Fixed prefab: $($_.Name)"
    }
}

$clipCount = 0
@(
    "Missile_Idle.anim",
    "Missile_Fire.anim",
    "Missile_Reload.anim",
    "Missile_Install.anim",
    "Missile_Remove.anim"
) | ForEach-Object {
    $clipPath = Join-Path $clipFolder $_
    if (-not (Test-Path $clipPath)) {
        return
    }

    if (Fix-FirearmAnimationY -Path $clipPath) {
        $clipCount++
        Write-Host "Fixed clip: $_"
    }
}

Write-Host "Done. Prefabs=$prefabCount Clips=$clipCount"
