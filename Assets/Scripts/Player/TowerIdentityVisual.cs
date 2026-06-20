using Tower;

using UnityEngine;

using UnityEngine.Rendering;



namespace Player

{

    [DisallowMultipleComponent]

    public class TowerIdentityVisual : MonoBehaviour

    {

        [SerializeField] private float ringDiameter = 2.1f;

        [SerializeField] private float ringHeight = 0.06f;



        private AbstractPlayer _owner;

        private Transform _ringTransform;

        private Renderer _ringRenderer;

        private Material _ringMaterial;



        public void Apply(AbstractPlayer owner)

        {

            _owner = owner;

            EnsureRing();

            Refresh();

        }



        public void Refresh()

        {

            if (_owner == null)

                return;



            TowerGrade grade = _owner.Grade;

            Color ringColor = TowerIdentityPalette.GetGradeRingColor(grade);

            float ringScale = TowerIdentityPalette.GetGradeRingScale(grade);



            if (_ringTransform != null)

            {

                _ringTransform.localPosition = new Vector3(0f, ringHeight * 0.5f, 0f);

                _ringTransform.localScale = new Vector3(ringDiameter * ringScale, ringHeight, ringDiameter * ringScale);

            }



            ApplyRingColor(ringColor);

        }



        private void EnsureRing()

        {

            if (_ringTransform != null)

                return;



            GameObject ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            ringObject.name = "TowerGradeRing";

            ringObject.transform.SetParent(transform, false);

            ringObject.transform.localPosition = new Vector3(0f, ringHeight * 0.5f, 0f);

            ringObject.transform.localScale = new Vector3(ringDiameter, ringHeight, ringDiameter);



            Collider collider = ringObject.GetComponent<Collider>();

            if (collider != null)

                Destroy(collider);



            _ringTransform = ringObject.transform;

            _ringRenderer = ringObject.GetComponent<Renderer>();



            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)

                shader = Shader.Find("Standard");



            _ringMaterial = new Material(shader)

            {

                hideFlags = HideFlags.HideAndDontSave,

                renderQueue = (int)RenderQueue.GeometryLast + 1

            };



            if (_ringMaterial.HasProperty("_Surface"))

                _ringMaterial.SetFloat("_Surface", 1f);



            if (_ringMaterial.HasProperty("_EmissionColor"))

                _ringMaterial.EnableKeyword("_EMISSION");



            _ringRenderer.sharedMaterial = _ringMaterial;

            _ringRenderer.shadowCastingMode = ShadowCastingMode.Off;

            _ringRenderer.receiveShadows = false;

        }



        private void ApplyRingColor(Color color)

        {

            if (_ringMaterial == null)

                return;



            if (_ringMaterial.HasProperty("_BaseColor"))

                _ringMaterial.SetColor("_BaseColor", color);

            else

                _ringMaterial.color = color;



            if (_ringMaterial.HasProperty("_EmissionColor"))

                _ringMaterial.SetColor("_EmissionColor", color * 0.65f);

        }



        private void OnDestroy()

        {

            if (_ringMaterial != null)

                Destroy(_ringMaterial);

        }

    }

}


