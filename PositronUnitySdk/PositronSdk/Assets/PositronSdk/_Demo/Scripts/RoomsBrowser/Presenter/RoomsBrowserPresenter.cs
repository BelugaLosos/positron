using Positron.Client;
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

        private bool _isErrorDisconnection;

        public RoomsBrowserPresenter(RoomsBrowserModel model, RoomsBrowserView view)
        {
            _model = model;
            _view = view;

            _model.recievedRoomsList += OnReceivedRoomsList;
            _view.pressedRefrash += OnPressRefrash;
            _view.pressedJoin += PressJoin;
            _view.pressCreate += PressCreateButton;

#pragma warning disable UDR0005 // Domain Reload Analyzer
            PositronFacade.connectionRequested += OnConeectionRequested;
            PositronFacade.connected += OnConnected;
            PositronFacade.disconnected += OnDisconnected;
            PositronFacade.connectionResetted += OnConnectionReset;
#pragma warning restore UDR0005 // Domain Reload Analyzer

            _view.Init();
        }

        public void Dispose()
        {
            _model.recievedRoomsList -= OnReceivedRoomsList;
            _view.pressedRefrash -= OnPressRefrash;
            _view.pressedJoin -= PressJoin;
            _view.pressCreate -= PressCreateButton;
            PositronFacade.connectionRequested -= OnConeectionRequested;
            PositronFacade.connected -= OnConnected;
            PositronFacade.disconnected -= OnDisconnected;
            PositronFacade.connectionResetted -= OnConnectionReset;

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

        private void OnConeectionRequested()
        {
            _view.DisplayStatus("Connection to server...");
        }

        private void OnConnected()
        {
            _view.HideStatus();
        }

        private void OnDisconnected()
        {
            if (_isErrorDisconnection)
            {
                return;
            }

            _view.DisplayStatus("Disconnected ...");
        }

        private void OnConnectionReset(ConnectionResetError error)
        {
            _view.DisplayStatus($"Disconnected with error: {error}");
            _isErrorDisconnection = true;
        }
    }
}