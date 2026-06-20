$project = "C:\Users\admin\TowerDefense"
$runGuid = "9aa393fe43ac256419d901567b75c774"
$dieGuid = "e3f4a5b6c7d84990abcdef3333333333"
$dragonRunGuid = "ef24c49b569124f42be6a28c58027ef0"
$dragonDieGuid = "ff3a2243eee7e27498572307676d3dbf"

$enemies = @(
    @{ Name = "Enemy_Orc"; VisualGuid = "196cb74b233e488796423925cf1a7d66"; GoId = "8511647367374691266"; TrId = "8511647367374981090"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f601"; Scale = 1.5 },
    @{ Name = "Enemy_Golem"; VisualGuid = "e882d275bf7f4e53a524afc446755b99"; GoId = "1706760125350986"; TrId = "4191744185429214"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f602"; Scale = 1.0 },
    @{ Name = "Enemy_Skeleton"; VisualGuid = "89dcc4601fd1424bbdb6ccc297185ab5"; GoId = "6285209290496832306"; TrId = "6285209290496798482"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f603"; Scale = 1.8 },
    @{ Name = "Enemy_Spider"; VisualGuid = "4a7d79c32a2d4de88831b8812bc8bc4e"; GoId = "1557585983417840"; TrId = "4843028258353612"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f604"; Scale = 2.0 },
    @{ Name = "Enemy_Bat"; VisualGuid = "d34ea5c7a264441f89039c4bc34ef4c3"; GoId = "5122791751604650877"; TrId = "5122791751604887389"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f605"; Scale = 2.5 },
    @{ Name = "Enemy_TurtleShell"; VisualGuid = "f3d4e5e80355479b9e6a756a94c77fae"; GoId = "1524411113276028"; TrId = "4544219117696442"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f606"; Scale = 1.5 },
    @{ Name = "Enemy_MonsterPlant"; VisualGuid = "433a831b00ce40d38355ed87cd4effd8"; GoId = "1748489546896114"; TrId = "4700302584741114"; PrefabGuid = "a1b2c3d4e5f6478990a1b2c3d4e5f607"; Scale = 1.5 }
)

$bosses = @(
    @{ Name = "Enemy_Boss_Orc"; VisualGuid = "196cb74b233e488796423925cf1a7d66"; GoId = "8511647367374691266"; TrId = "8511647367374981090"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f701"; Scale = 1.8 },
    @{ Name = "Enemy_Boss_Golem"; VisualGuid = "e882d275bf7f4e53a524afc446755b99"; GoId = "1706760125350986"; TrId = "4191744185429214"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f702"; Scale = 1.25 },
    @{ Name = "Enemy_Boss_Skeleton"; VisualGuid = "89dcc4601fd1424bbdb6ccc297185ab5"; GoId = "6285209290496832306"; TrId = "6285209290496798482"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f703"; Scale = 2.2 },
    @{ Name = "Enemy_Boss_Spider"; VisualGuid = "4a7d79c32a2d4de88831b8812bc8bc4e"; GoId = "1557585983417840"; TrId = "4843028258353612"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f704"; Scale = 2.4 },
    @{ Name = "Enemy_Boss_Bat"; VisualGuid = "d34ea5c7a264441f89039c4bc34ef4c3"; GoId = "5122791751604650877"; TrId = "5122791751604887389"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f705"; Scale = 3.0 },
    @{ Name = "Enemy_Boss_TurtleShell"; VisualGuid = "f3d4e5e80355479b9e6a756a94c77fae"; GoId = "1524411113276028"; TrId = "4544219117696442"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f706"; Scale = 1.9 },
    @{ Name = "Enemy_Boss_MonsterPlant"; VisualGuid = "433a831b00ce40d38355ed87cd4effd8"; GoId = "1748489546896114"; TrId = "4700302584741114"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f707"; Scale = 1.9 },
    @{ Name = "Enemy_Boss_Dragon"; VisualGuid = "fd98776f40a64187a54c07952cde7f71"; GoId = "5611543388551634691"; TrId = "5611543388551136099"; PrefabGuid = "b1b2c3d4e5f6478990a1b2c3d4e5f708"; Scale = 0.75; RunAnimGuid = $dragonRunGuid; DieAnimGuid = $dragonDieGuid; CcHeight = 4; CcRadius = 1.2 }
)

function New-EnemyPrefabContent([hashtable]$e) {
    $scale = $e.Scale
    $runAnimGuid = if ($e.RunAnimGuid) { $e.RunAnimGuid } else { $runGuid }
    $dieAnimGuid = if ($e.DieAnimGuid) { $e.DieAnimGuid } else { $dieGuid }
    $ccHeight = if ($e.CcHeight) { $e.CcHeight } else { 2 }
    $ccRadius = if ($e.CcRadius) { $e.CcRadius } else { 0.5 }
    $ccCenterY = $ccHeight / 2
@"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &475830405676779459
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 41116935039139627}
  - component: {fileID: 2802479070094959389}
  - component: {fileID: 2874766966885212515}
  - component: {fileID: 4928974385091677516}
  - component: {fileID: 7265156065374168344}
  m_Layer: 6
  m_Name: $($e.Name)
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &41116935039139627
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 475830405676779459}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: -3, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 6019419499222972660}
  - {fileID: 3263674928585170094}
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &2802479070094959389
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 475830405676779459}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: b737164994e7f2847a1698bd48f83985, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Enemy
  OnHit:
    m_PersistentCalls:
      m_Calls: []
  OnDeath:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {fileID: 2802479070094959389}
        m_TargetAssemblyTypeName: Enemy, Assembly-CSharp
        m_MethodName: Remove
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {fileID: 0}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2
  runAnim: {fileID: 11400000, guid: $runAnimGuid, type: 2}
  dieAnim: {fileID: 11400000, guid: $dieAnimGuid, type: 2}
--- !u!114 &2874766966885212515
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 475830405676779459}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 0533e522af7007848b0684e028d8ef69, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::SplineMove
  splineContainer: {fileID: 0}
  <moveSpeed>k__BackingField: 5
  rotateAlongPath: 1
  rotationSpeed: 10
--- !u!143 &4928974385091677516
CharacterController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 475830405676779459}
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Height: $ccHeight
  m_Radius: $ccRadius
  m_SlopeLimit: 45
  m_StepOffset: 0.3
  m_SkinWidth: 0.08
  m_MinMoveDistance: 0.001
  m_Center: {x: 0, y: $ccCenterY, z: 0}
--- !u!114 &7265156065374168344
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 475830405676779459}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 985999e70c7eefd4ea46d1721b9c7cba, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::EnemyEconomyBridge
  rewardConfig: {fileID: 11400000, guid: 9f8e7d6c5b4a39281706152433445566, type: 2}
--- !u!1 &3189137815068952672
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 6019419499222972660}
  - component: {fileID: 5872640202932694305}
  m_Layer: 6
  m_Name: HealthModule
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &6019419499222972660
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 3189137815068952672}
  serializedVersion: 2
  m_LocalRotation: {x: -0, y: -0, z: -0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 41116935039139627}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &5872640202932694305
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 3189137815068952672}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2d66f19183fd44b7b47aea5deb44f1fc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::CombatSystem.HealthModule
  <maxHealth>k__BackingField: 50
  currentHealth: 0
--- !u!1001 &3267969361551503894
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 41116935039139627}
    m_Modifications:
    - target: {fileID: $($e.GoId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_Name
      value: EnemyVisual
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalScale.x
      value: $scale
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalScale.y
      value: $scale
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalScale.z
      value: $scale
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalPosition.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalPosition.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalRotation.w
      value: 1
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalRotation.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalRotation.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalRotation.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalEulerAnglesHint.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalEulerAnglesHint.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
      propertyPath: m_LocalEulerAnglesHint.z
      value: 0
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents:
    - targetCorrespondingSourceObject: {fileID: $($e.GoId), guid: $($e.VisualGuid), type: 3}
      insertIndex: -1
      addedObject: {fileID: 8995707459938294489}
    - targetCorrespondingSourceObject: {fileID: $($e.GoId), guid: $($e.VisualGuid), type: 3}
      insertIndex: -1
      addedObject: {fileID: 1609547628807237772}
  m_SourcePrefab: {fileID: 100100000, guid: $($e.VisualGuid), type: 3}
--- !u!4 &3263674928585170094 stripped
Transform:
  m_CorrespondingSourceObject: {fileID: $($e.TrId), guid: $($e.VisualGuid), type: 3}
  m_PrefabInstance: {fileID: 3267969361551503894}
  m_PrefabAsset: {fileID: 0}
--- !u!1 &3268631653762794062 stripped
GameObject:
  m_CorrespondingSourceObject: {fileID: $($e.GoId), guid: $($e.VisualGuid), type: 3}
  m_PrefabInstance: {fileID: 3267969361551503894}
  m_PrefabAsset: {fileID: 0}
--- !u!114 &8995707459938294489
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 3268631653762794062}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 81755245bbc64fdca92ed39dad435c9e, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Agents.AgentRenderer
--- !u!114 &1609547628807237772
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 3268631653762794062}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: e7064fb5b21248c89b832fe75b242630, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::Agents.AgentTrigger
"@
}

$prefabDir = Join-Path $project "Assets\Prefab\Enemy"
foreach ($e in $enemies) {
    $path = Join-Path $prefabDir "$($e.Name).prefab"
    New-EnemyPrefabContent $e | Set-Content $path -Encoding UTF8
    $metaPath = "$path.meta"
    if (-not (Test-Path $metaPath)) {
        @"
fileFormatVersion: 2
guid: $($e.PrefabGuid)
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content $metaPath -Encoding UTF8
    }
}

foreach ($b in $bosses) {
    $path = Join-Path $prefabDir "$($b.Name).prefab"
    New-EnemyPrefabContent $b | Set-Content $path -Encoding UTF8
    $metaPath = "$path.meta"
    @"
fileFormatVersion: 2
guid: $($b.PrefabGuid)
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content $metaPath -Encoding UTF8
}

$cutePetPrefabs = @("Enemy_Cat","Enemy_Dog","Enemy_Rabbit","Enemy_Sheep","Enemy_Cow","Enemy_Chick","Enemy_Duck")
foreach ($name in $cutePetPrefabs) {
    $p = Join-Path $prefabDir "$name.prefab"
    $m = Join-Path $prefabDir "$name.prefab.meta"
    if (Test-Path $p) { Remove-Item $p -Force }
    if (Test-Path $m) { Remove-Item $m -Force }
}

$spawnList = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: ce0583824d4705e448904aea0462291d, type: 3}
  m_Name: SpawnEnemyListSO
  m_EditorClassIdentifier: Assembly-CSharp::SpawnEnemyListSO
  enemy:
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f601, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f602, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f603, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f604, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f605, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f606, type: 3}
  - {fileID: 475830405676779459, guid: a1b2c3d4e5f6478990a1b2c3d4e5f607, type: 3}
  bossEnemy:
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f701, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f702, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f703, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f704, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f705, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f706, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f707, type: 3}
  - {fileID: 475830405676779459, guid: b1b2c3d4e5f6478990a1b2c3d4e5f708, type: 3}
"@
$spawnList | Set-Content (Join-Path $project "Assets\SO\Spawn\EnemySpawn\SpawnEnemyListSO.asset") -Encoding UTF8

$suriyun = Join-Path $project "Assets\Suriyun"
if (Test-Path $suriyun) { Remove-Item $suriyun -Recurse -Force }

Write-Output "RPG enemy and boss prefabs generated."
