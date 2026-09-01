using UnityEngine;

namespace Positron.Client.NetValues
{
    public interface INetValueCarrier
    {
        INetValueManaged[] GetNetValues();
    }
}