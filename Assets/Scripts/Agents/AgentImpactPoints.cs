using CoreSystem.EffectSystem;
using UnityEngine;

namespace Agents
{
    public static class AgentImpactPoints
    {
        private const float HeadHeightRatio = 0.88f;

        public static Vector3 Resolve(Agent agent, VfxImpactPlacement placement, float fallbackHeight = 1f)
        {
            switch (placement)
            {
                case VfxImpactPlacement.Head:
                    return GetHead(agent, fallbackHeight);
                case VfxImpactPlacement.Ground:
                    return GetGround(agent);
                default:
                    return GetBodyCenter(agent, fallbackHeight);
            }
        }

        public static VfxImpactPlacement ResolvePlacement(
            TowerAttackVfxDataSO hitVfx,
            bool useGroundImpactPoint)
        {
            if (useGroundImpactPoint)
                return VfxImpactPlacement.Ground;

            return hitVfx != null
                ? hitVfx.impactPlacement
                : VfxImpactPlacement.BodyCenter;
        }

        public static Vector3 GetBodyCenter(Agent agent, float fallbackHeight = 1f)
        {
            if (TryGetVisualBounds(agent, out Bounds bounds))
                return bounds.center;

            CharacterController controller = agent.GetComponent<CharacterController>();
            if (controller != null)
                return agent.transform.TransformPoint(controller.center);

            return agent.transform.position + Vector3.up * fallbackHeight;
        }

        public static Vector3 GetHead(Agent agent, float fallbackHeight = 1f)
        {
            if (TryGetVisualBounds(agent, out Bounds bounds))
            {
                float headY = Mathf.Lerp(bounds.center.y, bounds.max.y, HeadHeightRatio);
                return new Vector3(bounds.center.x, headY, bounds.center.z);
            }

            CharacterController controller = agent.GetComponent<CharacterController>();
            if (controller != null)
            {
                Vector3 localHead = controller.center
                    + Vector3.up * (controller.height * 0.5f - controller.radius * 0.3f);
                return agent.transform.TransformPoint(localHead);
            }

            return agent.transform.position + Vector3.up * (fallbackHeight + 0.5f);
        }

        private const float GroundImpactYOffset = 0.08f;
        private static int _groundRaycastMask = -1;

        public static Vector3 GetGround(Agent agent)
        {
            Vector3 point = agent.transform.position;
            if (TryGetVisualBounds(agent, out Bounds bounds))
            {
                point.x = bounds.center.x;
                point.z = bounds.center.z;
            }

            float surfaceY = GetFeetHeight(agent);
            if (TrySampleGroundSurface(point, agent, surfaceY, out float groundY))
                surfaceY = groundY;

            point.y = surfaceY + GroundImpactYOffset;
            return point;
        }

        private static float GetFeetHeight(Agent agent)
        {
            CharacterController controller = agent.GetComponent<CharacterController>();
            if (controller != null)
            {
                Vector3 localFeet = controller.center - Vector3.up * (controller.height * 0.5f);
                return agent.transform.TransformPoint(localFeet).y;
            }

            return agent.transform.position.y;
        }

        private static bool TrySampleGroundSurface(Vector3 horizontalPoint, Agent agent, float referenceHeight, out float groundY)
        {
            groundY = referenceHeight;

            float rayStartY = referenceHeight + 2f;
            if (TryGetVisualBounds(agent, out Bounds bounds))
                rayStartY = Mathf.Max(rayStartY, bounds.max.y + 0.5f);

            EnsureGroundRaycastMask();
            Vector3 origin = new Vector3(horizontalPoint.x, rayStartY, horizontalPoint.z);
            float maxDistance = rayStartY - referenceHeight + 12f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance, _groundRaycastMask, QueryTriggerInteraction.Ignore))
            {
                groundY = hit.point.y;
                return true;
            }

            return false;
        }

        private static void EnsureGroundRaycastMask()
        {
            if (_groundRaycastMask >= 0)
                return;

            _groundRaycastMask = LayerMask.GetMask("Default", "Ground", "Tile");
            if (_groundRaycastMask == 0)
                _groundRaycastMask = Physics.DefaultRaycastLayers;
        }

        public static bool TryGetVisualBounds(Agent agent, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in agent.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || !renderer.enabled || renderer is ParticleSystemRenderer)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }
    }
}
