using Positron.Client.DataTransferObjects;
using Positron.Demo.RoomsBrowser.Model;
using Positron.Demo.RoomsBrowser.Views;
using System;

namespace Positron.Demo.RoomsBrowser.Presenter
{
    public sealed class RoomsBrowserPresenter : IDisposable
    {
        private readonly RoomsBrowserModel _model;
        private readonly RoomsBrowserView _view;

        public RoomsBrowserPresenter(RoomsBrowserModel model, RoomsBrowserView view)
        {
            _model = model;
            _view = view;

            _model.recievedRoomsList += OnReceivedRoomsList;
            _view.pressedRefrash += OnPressRefrash;
            _view.pressedJoin += PressJoin;
            _view.pressCreate += PressCreateButton;

            _view.Init();
        }

        public void Dispose()
        {
            _model.recievedRoomsList -= OnReceivedRoomsList;
            _view.pressedRefrash -= OnPressRefrash;
            _view.pressedJoin -= PressJoin;
            _view.pressCreate -= PressCreateButton;

            _model.Dispose();
        }

        private void OnReceivedRoomsList(RoomListResponse response)
        {
            _view.Display(response);
        }

        private void PressJoin(string uuid)
        {
            _model.JoinRoom(uuid);
        }

        private void PressCreateButton(string name, int playersCap, int level)
        {
            _model.CreateRoom(name, playersCap, level);
        }

        private void OnPressRefrash()
        {
            _model.RefreshRooms();
        }
    }
}