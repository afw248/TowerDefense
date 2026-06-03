using UnityEngine;

namespace GGMLib.CoreLib
{
    [CreateAssetMenu(fileName = "Asset name", menuName = "Lib/Asset name", order = 15)]
    public class AssetNameSO : ScriptableObject
    {
        [field: SerializeField] public string AssetName { get; private set; }
        [field: SerializeField] public int AssetHash { get; private set; }

        private void OnValidate()
        {
            if(!string.IsNullOrEmpty(AssetName))
                AssetHash = Animator.StringToHash(AssetName);
        }
    }
}