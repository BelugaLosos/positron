using Positron.Extras.RoomsBrowser.Model;
using Positron.Extras.RoomsBrowser.Presenter;
using Positron.Extras.RoomsBrowser.Views;
using UnityEngine;

namespace Positron.Extras.RoomsBrowser
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