namespace PositronCodeGen.ConstantsHolder
{
    internal static class ConstantsHolderContainer
    {
        public static readonly string RPC_ATTR_NAME = "RpcAttribute";
        public static readonly string RPC_TARGETS_INTERFACE_DEFINITION = "global::Positron.Client.Rpc.IRpcTarget";
        public static readonly string NETWORK_READER_DEFINITION = "global::Positron.NetworkIoAPI.PositronNetworkReader";
        public static readonly string NETWORK_WRITER_DEFINITION = "global::Positron.NetworkIoAPI.PositronNetworkWriter";
        public static readonly string NETWORK_IO_POOL_DEFINITION = "global::Positron.PositronFacade.NetworkIoPool";
        public static readonly string NETWORK_IO_POOL_GET_WRITTER_METHOD = "GetWriter";
        public static readonly string NETWORK_IO_POOL_PUT_READER_METHOD = "PutReader";
        public static readonly string NETWORK_RPCS_MODEL_SEND_TO_SERVER_METHOD = "global::Positron.PositronFacade.World.RpcModel.SendRpcToServer";
        public static readonly string SUITABILITY_METHOD_DEFINITION = "IsSuitableTargetFor";
        public static readonly string CALL_METHOD_DEFINITION = "Call";
        public static readonly string RPC_READ_PREFIX = "CODEGEN_SERVICE_METHOD_ReadRPC_";
        public static readonly string RPC_SEND_PREFIX = "SendRPC_";
        public static readonly string RPC_TARGETS_ENUM_DEFINITION = "global::Positron.Client.ConstantHolders.RpcTargets";
        public static readonly string RPC_TARGETS_ENUM_VALUE_RPC_ALL = "RPC_ALL";
        public static readonly string RPC_TARGETS_ENUM_VALUE_RPC_ALL_CACHED = "RPC_ALL_CACHED";
        public static readonly string RPC_SPECIFIED_TARGET_STRUCT_NAME = "global::Positron.Client.Rpc.RpcPlayerRef";
        public static readonly string POSITRON_NETWORK_IDENTITY_DEFINITION = "global::Positron.Client.Mono.PositronNetworkIdentity";
    }
}
