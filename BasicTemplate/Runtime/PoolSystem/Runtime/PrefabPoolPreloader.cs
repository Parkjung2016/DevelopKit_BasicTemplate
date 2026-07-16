using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem
{
    [AddComponentMenu("PJDev/Basic Template/Prefab Pool Preloader")]
    public sealed class PrefabPoolPreloader : MonoBehaviour
    {
        [SerializeField] private PrefabPoolSettingsSO settings = null;

        private void Awake()
        {
            if (settings != null)
                settings.Prewarm();
        }
    }
}