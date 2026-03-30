using Positron.Demo.RoomsBrowser.Model;
using Positron.Demo.RoomsBrowser.Presenter;
using Positron.Demo.RoomsBrowser.Views;
using UnityEngine;

namespace Positron.Demo.RoomsBrowser
{
    public sealed class RoomsBrowserBootstrapper : MonoBehaviour
    {
        [SerializeField] private RoomsBrowserView _browserView;

        private RoomsBrowserPresenter _presenter;
        private RoomsBrowserModel _model;

        private void Awake()
        {
            _model = new();
            _presenter = new(_model, _browserView);
        }

        private void OnDestroy()
        {
            _presenter.Dispose();
        }
    }
}