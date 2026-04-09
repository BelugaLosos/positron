using Cysharp.Threading.Tasks;
using Positron.Client;
using Positron.Client.Handlers;
using Positron.Client.Ping;
using Positron.Client.Settings;
using Positron.Serialzier;
using Positron.Transport;
using Positron.Client.ConstantHolders;
using UnityEngine;
using System;
using Positron.Client.DataTransferObjects;
using Positron.Client.Interfaces;

namespace Positron
{
    public static class PositronFacade
    {
        private static PositronClient _client;
        private static MonoHook _monoHook;
        private static PositronSettings _settings;

        private static PingModel _pingModel;

        private static bool _initialized;
        private static bool _connected;
        private static bool _pending;

        public static IReadOnlyPingModel PingModel => _pingModel;
        public static IPositronObservableHandler<RoomListResponse> GetRoomsHandler => _client.GetHandler<GetRoomsHandler>();
        public static IPositronObservableHandler<RoomCreationResponse> RoomCreatedHandler => _client.GetHandler<RoomCreatedHandler>();

        public static event Action connected;
        public static event Action disconnected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticFileds()
        {
            if (_monoHook != null)
            {
                GameObject.DestroyImmediate(_monoHook);
            }

            if (_pingModel != null) 
            {
                _pingModel.Dispose();
            }

            if (_connected)
            {
                Disconnect();
            }
            else
            {
                if (_client != null)
                {
                    _client.Dispose();
                }
            }

            _client = null;
            _monoHook = null;
            _settings = null;
            _pingModel = null;
            _initialized = false;
            _connected = false;
            _pending = false;

            Debug.Log("Positron hooked and handled domain reload");
        }

        public static void InitSdk(PositronSettings settings)
        {
            if (_initialized)
            {
                Debug.LogError("Initialized after initialized");
                return;
            }

            _settings = settings;

            _pingModel = new();
            _client = new
                (
                    settings, new MsgPackSerializer(), new WebSocketTransport(), 
                    new PingHandler(_pingModel),
                    new GetRoomsHandler(),
                    new RoomCreatedHandler()
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
            _client.Dispose();
            _pending = true;
        }

        public static void GetRoomsList()
        {
            _client.SendRaw(stackalloc byte[1] { 0xFF }, EventTypes.GET_ALL_ROOMS, true);
        }

        public static void JoinRoom(string uuid)
        {
            JoinRoomRequest request = new();
            request.Uuid = uuid;

            _client.Send(request, EventTypes.JOIN_ROOM, true);
        }

        public static void CreateRoom(string name, int playerCap, int level, byte[] externalData)
        {
            CreateRoomPacket request = new();
            request.Name = name;
            request.PlayerCap = (uint)playerCap;
            request.Scene = (uint)level;
            request.Tickrate = (uint)_settings.Tickrate;
            request.ExternalData = externalData;

            _client.Send(request, EventTypes.CREATE_ROOM, true);
        }

        private static void OnConnected()
        {
            Debug.Log("Positron connected");
            _connected = true;
            _pending = false;

            connected?.Invoke();
        }

        private static void OnDisconnected()
        {
            Debug.Log("Positron disconnected");
            _connected = false;
            _pending = false;

            _pingModel.Dispose();

            disconnected?.Invoke();
        }
    }
}