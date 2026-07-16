using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime.PoolSystem.Addressables
{
    [AddComponentMenu("PJDev/Basic Template/Addressable Pool Preloader")]
    public sealed class AddressablePoolPreloader : MonoBehaviour
    {
        [SerializeField] private AddressablePoolSettingsSO settings = null;

        public Task PreloadTask { get; private set; } = Task.CompletedTask;

        private void Awake()
        {
            if (settings == null)
                return;

            PreloadTask = settings.PrewarmAsync();
            ObservePreload(PreloadTask);
        }

        private async void ObservePreload(Task preloadTask)
        {
            try
            {
                await preloadTask;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
