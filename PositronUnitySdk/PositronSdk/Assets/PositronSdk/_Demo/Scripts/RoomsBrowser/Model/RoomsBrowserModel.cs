using Positron.Client.DataTransferObjects;
using System;
using UnityEngine;

namespace Positron.Demo.RoomsBrowser.Model
{
    public sealed class RoomsBrowserModel : IDisposable
    {
        public event Action<RoomListResponse> recievedRoomsList;

        public RoomsBrowserModel()
        {
            PositronFacade.GetRoomsHandler.callback += OnReceiveRooms;
        }

        public void Dispose()
        {
            PositronFacade.GetRoomsHandler.callback -= OnReceiveRooms;
        }

        public void RefreshRooms()
        {
            PositronFacade.GetRoomsList();
        }

        public void JoinRoom(string roomUuid)
        {
            Debug.Log(roomUuid);
        }

        public void CreateRoom(string name, int playerCap, int level)
        {
            Debug.Log($"Need implement create n:{name} p:{playerCap} l:{level}");
        }

        private void OnReceiveRooms(RoomListResponse response)
        {
            recievedRoomsList?.Invoke(response);
        }
    }
}