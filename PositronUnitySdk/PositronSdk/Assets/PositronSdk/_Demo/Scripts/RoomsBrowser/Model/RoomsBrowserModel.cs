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
            PositronFacade.RoomCreatedHandler.callback += OnRoomCreated;
        }

        public void Dispose()
        {
            PositronFacade.GetRoomsHandler.callback -= OnReceiveRooms;
            PositronFacade.RoomCreatedHandler.callback -= OnRoomCreated;
        }

        public void RefreshRooms()
        {
            PositronFacade.GetRoomsList();
        }

        public void JoinRoom(string roomUuid)
        {
            PositronFacade.JoinRoom(roomUuid);
        }

        public void CreateRoom(string name, int playerCap, int level)
        {
            PositronFacade.CreateRoom(name, playerCap, level, new byte[0]);
        }

        private void OnReceiveRooms(RoomListResponse response)
        {
            recievedRoomsList?.Invoke(response);
        }

        private void OnRoomCreated(RoomCreationResponse response)
        {
            Debug.Log("Created room");
            RefreshRooms();
        }
    }
}