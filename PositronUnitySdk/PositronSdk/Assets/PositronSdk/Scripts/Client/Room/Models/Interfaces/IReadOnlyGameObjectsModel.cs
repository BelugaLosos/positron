using Positron.Client.Mono;
using System;

namespace Positron.Client.Room.Models.Interfaces
{
    public interface IReadOnlyGameObjectsModel
    {
        event Action<PositronNetworkIdentity> localObjectGoingRemove;
        event Action<PositronNetworkIdentity> localObjectRemotelyInitedSuccessfully;

        bool HasObjectInCreationDeltaNow(ushort creationId);
        PositronNetworkIdentity GetObjectByIds(uint objectId, ushort subObjectId);
    }
}