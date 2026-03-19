using Cysharp.Threading.Tasks;
using Positron.Client;
using Positron.Client.Handlers;
using Positron.Client.Ping;
using Positron.Client.Settings;
using Positron.Serialzier;
using Positron.Transport;
using UnityEngine;

namespace Positron
{
    public static class PositronFacade
    {
        private static PositronClient _client;
        private static MonoHook _monoHook;

        private static PingModel _pingModel;

        private static bool _initialized;
        private static bool _connected;
        private static bool _pending;

        public static IReadOnlyPingModel PingModel => _pingModel;

        public static void InitSdk(PositronSettings settings)
        {
            if (_initialized)
            {
                Debug.LogError("Initialized after initialized");
                return;
            }

            _pingModel = new();
            _client = new
                (
                    settings, new MsgPackSerializer(), new WebSocketTransport(), 
                    new PingHandler(_pingModel)
                );

            _pingModel.Init(_client);

            _pingModel.EstimationLoop().Forget();

            _monoHook = new GameObject("PositronMonoHook").AddComponent<MonoHook>();
            GameObject.DontDestroyOnLoad(_monoHook);

            _monoHook.destroyed += Disconnect;
            _client.connected += OnConnected;
            _client.disconnected += OnDisconnected;

            _initialized = true;
        }

        public static void Connect()
        {
            if (!_initialized)
            {
                Debug.LogError("Cant connect with not initialized SDK");
                return;
            }

            if (_connected)
            {
                return;
            }

            if (_pending)
            {
                return;
            }

            Debug.Log("Connecting...");
            _client.Connect();
            _pending = true;
        }

        public static void Disconnect()
        {
            if (!_connected)
            {
                return;
            }

            if (_pending)
            {
                return;
            }

            Debug.Log("Disconnecting...");
            _client.Disconnect();
            _pending = true;
        }

        private static void OnConnected()
        {
            Debug.Log("Positron connected");
            _connected = true;
            _pending = false;
        }

        private static void OnDisconnected()
        {
            Debug.Log("Positron disconnected");
            _connected = false;
            _pending = false;
        }
    }
}