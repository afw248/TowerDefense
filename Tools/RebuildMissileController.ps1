$srcPath = Join-Path $PSScriptRoot "..\Assets\Graphics\FattyPolyTurretPart2\Animations\Catapult.controller"
$dstPath = Join-Path $PSScriptRoot "..\Assets\Graphics\FattyPolyTurretPart2\Animations\Missile.controller"
$content = Get-Content $srcPath -Raw
$content = $content.Replace('m_Name: Catapult_Install', 'm_Name: INSTALL')
$content = $content.Replace('m_Name: Catapult_Reload', 'm_Name: RELOAD')
$content = $content.Replace('m_Name: Catapult_Fire', 'm_Name: FIRE')
$content = $content.Replace('m_Name: Catapult_Idle', 'm_Name: IDLE')
$content = $content.Replace('m_Name: Catapult_Remove', 'm_Name: REMOVE')
$content = $content.Replace('m_Name: Catapult', 'm_Name: Missile')
$content = $content.Replace('guid: 1ba54e0a08beb364699ee23e423d5ea8', 'guid: 23ab36867ff0d8a49927b4d80dda5762')
$content = $content.Replace('guid: d6f5f2b1001d3e843af067739e2a7357', 'guid: a1e374ea04693774792a5b7d95adf345')
$content = $content.Replace('guid: 7a38a993cc4ad094c9e51a29a4cc3493', 'guid: 4fb15744652ff354d882b575edd8a6f6')
$content = $content.Replace('guid: 4b360fa66a91a2e49b48b445cebdd697', 'guid: b185c6550f7ba274f92e6422259e6761')
$content = $content.Replace('guid: 3fdd4dd2ce9c15246a3af5fe1e9d78bb', 'guid: 0037fbf822bc78840b163857d348f55a')
Set-Content -Path $dstPath -Value $content -NoNewline
