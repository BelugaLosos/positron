using Positron.Client.Settings;
using UnityEngine;

namespace Positron.Boot
{
    public static class PositronBootstrapper
    {
        private static bool _booted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBootGame()
        {
            if (_booted)
            {
                return;
            }

            PositronSettings settings = Resources.Load<PositronSettings>("PositronSettings");
            PositronFacade.InitSdk(settings);

            if (settings.Autoconnect)
            {
                PositronFacade.Connect();
            }

            _booted = true;
        }
    }
}