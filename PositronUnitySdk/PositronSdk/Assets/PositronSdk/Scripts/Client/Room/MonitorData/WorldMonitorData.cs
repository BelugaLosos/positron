namespace Positron.Client.Room.MonitorData
{
    public struct WorldMonitorData
    {
        public NetworkClockData ClockData { get; private set; }
        public GameObjectsModelData GameObjectsData { get; private set; }
        public NetValuesModelData NetValuesData { get; private set; }
        public RpcsModelData RpcsData { get; private set; }
    
        public WorldMonitorData(NetworkClockData cDat, GameObjectsModelData goDat, NetValuesModelData netvDat, RpcsModelData rDat)
        {
            ClockData = cDat;
            GameObjectsData = goDat;
            NetValuesData = netvDat;
            RpcsData = rDat;
        }
    }

    public struct NetworkClockData
    {
        public double RecentServerTime { get; private set; }
        public double ClientTime { get; private set; }
        public double TimeScale { get; private set; }
        public double FrameOffset { get; private set; }
        public double Drift { get; private set; }   

        public NetworkClockData(double rsTick, double cTick, double tScale, double frameOffset)
        {
            RecentServerTime = rsTick;
            ClientTime = cTick;
            TimeScale = tScale;
            FrameOffset = frameOffset;
            Drift = rsTick - cTick;
        }
    }

    public struct GameObjectsModelData
    {
        public int CreationDeltaCount { get; private set; }
        public int MoveDeltaCount { get; private set; }
        public int DestroyDeltaCount { get; private set; }
        public int OwnershipRequestDeltaCount { get; private set; }
        public int LocalCreationMappingSize { get; private set; }
        public int LocalCreationBlackListSize { get; private set; }
        public int CurrentGameObjectsOnSceneCount { get; private set; }

        public GameObjectsModelData(int creationDeltaCount, int moveDeltaCount, int destroyDeltaCount, int ownershipRequestDeltaCount, 
                                    int localCreationMappingSize, int localCreationBlackListSize, int currentGameObjectsOnSceneCount)
        {
            CreationDeltaCount = creationDeltaCount;
            MoveDeltaCount = moveDeltaCount;
            DestroyDeltaCount = destroyDeltaCount;
            OwnershipRequestDeltaCount = ownershipRequestDeltaCount;
            LocalCreationMappingSize = localCreationMappingSize;
            LocalCreationBlackListSize = localCreationBlackListSize;
            CurrentGameObjectsOnSceneCount = currentGameObjectsOnSceneCount;
        }
    }

    public struct NetValuesModelData
    {
        public int CurrentAddDeltaSize { get; private set; }
        public int CurrentModDeltaSize { get; private set; }
        public int OutgoingArenaSize { get; private set; }
        public int IncomingArenaSize { get; private set; }
        public int DescriptedWorldMemoryMappingSize { get; private set; }

        public NetValuesModelData(int currentAddDeltaSize, int currentModDeltaSize, int outgoingArenaSize, int incomingArenaSize, 
                                  int descriptedWorldMemoryMappingSize)
        {
            CurrentAddDeltaSize = currentAddDeltaSize;
            CurrentModDeltaSize = currentModDeltaSize;
            OutgoingArenaSize = outgoingArenaSize;
            IncomingArenaSize = incomingArenaSize;
            DescriptedWorldMemoryMappingSize = descriptedWorldMemoryMappingSize;
        }
    }

    public struct RpcsModelData
    {
        public int CallBufferSize { get; private set; }
        public int DellaydCallsBufferSize { get; private set; }
        public int RpcToObjectMemoryMappingSize { get; private set; }
        public int OutgoingArenaSize { get; private set; }
        public int IncomingArenaSize { get; private set; }

        public RpcsModelData(int callBufferSize, int dellaydCallsBufferSize, int rpcToObjectMemoryMappingSize, 
                             int outgoingArenaSize, int incomingArenaSize)
        {
            CallBufferSize = callBufferSize;
            DellaydCallsBufferSize = dellaydCallsBufferSize;
            RpcToObjectMemoryMappingSize = rpcToObjectMemoryMappingSize;
            OutgoingArenaSize = outgoingArenaSize;
            IncomingArenaSize = incomingArenaSize;
        }
    }
}