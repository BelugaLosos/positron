using Positron.Client.GameEntities;
using System;

namespace Positron.Client.Room.Models
{
    public sealed class NetworkValuesModel : IDisposable
    {
        public void Dispose()
        {
            ClearWorld();
        }

        public void ClearWorld()
        {

        }

        public void AddOrModifyValues(NetValue[] values)
        {
            foreach (NetValue value in values)
            {
                AddOrModifyValue(value);
            }
        }

        public void AddOrModifyValue(NetValue value)
        {

        }
    }
}