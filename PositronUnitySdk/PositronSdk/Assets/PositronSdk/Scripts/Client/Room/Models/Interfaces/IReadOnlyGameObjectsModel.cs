using Positron.Client.Mono;

namespace Positron.Client.Room.Models.Interfaces
{
    public interface IReadOnlyGameObjectsModel
    {
        PositronNetworkIdentity GetObjectByIds(uint objectId, ushort subObjectId);
    }
}