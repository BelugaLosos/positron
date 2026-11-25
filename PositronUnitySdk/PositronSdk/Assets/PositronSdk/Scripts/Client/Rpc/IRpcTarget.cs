using Positron.NetworkIoAPI;

namespace Positron.Client.Rpc
{
    public interface IRpcTarget
    {
        bool IsSuitableTargetFor(ulong name);
        void Call(ulong name, PositronNetworkReader reader);
#if UNITY_EDITOR
        RpcCallExportData[] ExportRpcRawAuthoritySchema();
#endif
    }
}