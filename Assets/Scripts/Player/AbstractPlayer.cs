using System;
using Agents;
using Agents.FSM;
using CombatSystem;
using FSM;
using Tower;
using UnityEngine;

namespace Player
{
    public abstract class AbstractPlayer : Agent
    {
        [field: SerializeField] public PlayerDataSO PlayerData { get; private set; } 
        public IRenderer Renderer { get; private set; }
        public ISensor Sensor { get; private set; }
        public ISkillModule SkillModule { get; private set; }
        public AgentTrigger Trigger { get; private set; } 
        [field:SerializeField]public StateListSO playerStates { get; private set; }

        public StateMachine _stateMachine;

        [SerializeField] private bool isDebugMode;

        [SerializeField] private float holdLiftHeight = 1.2f;

        [SerializeField] private TowerVariantSO towerVariant;

        private float _placementGroundY;
        private bool _isHeldForRelocation;
        private TowerHoldOutline _holdOutline;
        private TowerIdentityVisual _identityVisual;

        public TowerVariantSO TowerVariant => towerVariant;
        public TowerGrade Grade => towerVariant != null ? towerVariant.grade : TowerGrade.Normal;
        public TowerArchetype Archetype => towerVariant != null ? towerVariant.archetype : TowerArchetype.Bow;

        public float EffectiveAttack
        {
            get
            {
                float baseAttack = PlayerData != null ? PlayerData.Attack : 0f;
                TowerArchetype archetype = towerVariant != null
                    ? towerVariant.archetype
                    : TowerArchetype.Bow;

                float bonus = ArchetypeUpgradeManager.Instance != null
                    ? ArchetypeUpgradeManager.Instance.GetBonusAttack(archetype)
                    : 0f;

                return baseAttack + bonus;
            }
        }
        public bool IsHeldForRelocation => _isHeldForRelocation;
        public bool CanAttack => !_isHeldForRelocation;
        public float PlacementGroundY => _placementGroundY;

        public void BeginHoldForRelocation()
        {
            if (_isHeldForRelocation)
                return;

            _isHeldForRelocation = true;
            _placementGroundY = transform.position.y;

            SkillModule?.ForceStopAllSkills();
            SetHoldOutline(true);
            SetDragHighlight(true);

            Vector3 lifted = transform.position;
            lifted.y += holdLiftHeight;
            transform.position = lifted;

            ChangeState(PlayerState.IDLE, 0.05f);
        }

        public void CancelHoldForRelocation()
        {
            if (!_isHeldForRelocation)
                return;

            _isHeldForRelocation = false;
            RestoreGradeOutlineOrDisable();

            Vector3 grounded = transform.position;
            grounded.y = _placementGroundY;
            transform.position = grounded;

            ChangeState(PlayerState.IDLE, 0.1f);
        }

        public void UpdateDragPosition(Vector3 worldPosition)
        {
            if (!_isHeldForRelocation)
                return;

            transform.position = worldPosition;
        }

        public void ReturnDragToTile(Vector3 tileWorldPosition)
        {
            ResumeAfterDrag(tileWorldPosition);
        }

        public void ResumeAfterDrag(Vector3 tileWorldPosition)
        {
            if (!_isHeldForRelocation)
                return;

            _isHeldForRelocation = false;
            SetDragHighlight(false);
            RestoreGradeOutlineOrDisable();

            Vector3 grounded = tileWorldPosition;
            grounded.y = _placementGroundY;
            transform.position = grounded;

            SkillModule?.ForceStopAllSkills();

            if (HasEnemyInRange())
                ChangeState(PlayerState.FIRE, 0.05f);
            else
                ChangeState(PlayerState.IDLE, 0.05f);
        }

        public void SetDragHighlight(bool enabled)
        {
            EnsureHoldOutline()?.SetDragHighlight(enabled);
        }

        public void SetHoldFeedbackColor(Color color)
        {
            TowerHoldOutline outline = EnsureHoldOutline();
            outline?.SetOutlineColor(color);
            outline?.SetOutlineEnabled(true);
        }

        public void ResetHoldFeedbackColor()
        {
            if (_holdOutline == null || _isHeldForRelocation)
                return;

            _holdOutline.SetDragHighlight(false);
            _holdOutline.ResetOutlineColor();
            RestoreGradeOutlineOrDisable();
        }

        private bool HasEnemyInRange()
        {
            if (PlayerData == null || Sensor == null)
                return false;

            return Sensor.FindTargetsInRadius(PlayerData.DetectRadius) > 0;
        }

        public void InitializeSpawnPlacement(Vector3 worldPosition)
        {
            _placementGroundY = worldPosition.y;
            transform.position = worldPosition;
        }

        public void PlaceAfterHold(Vector3 worldPosition, float transitionDuration = 0.15f)
        {
            if (!_isHeldForRelocation)
                _placementGroundY = transform.position.y;

            _isHeldForRelocation = false;
            SetDragHighlight(false);
            RestoreGradeOutlineOrDisable();

            Vector3 placed = worldPosition;
            placed.y = _placementGroundY;
            transform.position = placed;

            ChangeState(PlayerState.INSTALL, transitionDuration);
        }

        public void RelocateTo(Vector3 worldPosition, float transitionDuration = 0.15f)
        {
            PlaceAfterHold(worldPosition, transitionDuration);
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();
            _stateMachine = new StateMachine(this, playerStates.states);
            Renderer = GetModule<IRenderer>();
            Sensor = GetModule<ISensor>();
            SkillModule = GetModule<ISkillModule>();
            Trigger = GetModule<AgentTrigger>();
        }

        protected override void AfterInitModules()
        {
            base.AfterInitModules();
            EnsureSelectionHitbox();
        }

        private void EnsureSelectionHitbox()
        {
            TowerSelectionHitbox hitbox = GetComponent<TowerSelectionHitbox>();
            if (hitbox == null)
                hitbox = gameObject.AddComponent<TowerSelectionHitbox>();

            hitbox.EnsureCollider();
        }
        public void OnEnable()
        {
            if (_stateMachine != null)
            {
                ChangeState(PlayerState.INSTALL, transitionDuration: 0);
            }
        }
        public void Update()
        {
            _stateMachine?.UpdateMachine();
        }
        private void OnDrawGizmos()
        {
            if (!isDebugMode) return;
            if (PlayerData == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, PlayerData.DetectRadius);
        }
        public void Remove()
        {
            Destroy(gameObject);
        }
        public void ChangeState(PlayerState newState, float transitionDuration)
        {
            _stateMachine?.ChangeState((int)newState, transitionDuration);
        }

        /// <summary>타워 발밑 등급 링과 등급 라벨을 표시합니다.</summary>
        public void EnableGradeOutline()
        {
            EnsureIdentityVisual()?.Apply(this);
        }

        private void RestoreGradeOutlineOrDisable()
        {
            SetHoldOutline(false);
        }

        private void SetHoldOutline(bool enabled)
        {
            TowerHoldOutline outline = EnsureHoldOutline();
            if (outline != null)
                outline.SetOutlineEnabled(enabled);
        }

        private TowerIdentityVisual EnsureIdentityVisual()
        {
            if (_identityVisual == null)
                _identityVisual = GetComponent<TowerIdentityVisual>();

            if (_identityVisual == null)
                _identityVisual = gameObject.AddComponent<TowerIdentityVisual>();

            return _identityVisual;
        }

        private TowerHoldOutline EnsureHoldOutline()
        {
            if (_holdOutline == null)
                _holdOutline = GetComponent<TowerHoldOutline>();

            if (_holdOutline == null)
                _holdOutline = gameObject.AddComponent<TowerHoldOutline>();

            _holdOutline?.EnsureInitialized();
            return _holdOutline;
        }
    }
}
