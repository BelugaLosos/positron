using System;
using System.Collections.Generic;
using Positron.Client.DataTransferObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Positron.Demo.RoomsBrowser.Views
{
    public sealed class RoomsBrowserView : MonoBehaviour
    {
        [SerializeField] private RectTransform _scrollOrigin;

        [Space]

        [SerializeField] private Button _refrashButton;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private TMP_InputField _roomNameField;
        [SerializeField] private TMP_Dropdown _playersCap;
        [SerializeField] private TMP_Dropdown _level;

        [Space]

        [SerializeField] private RoomCard _roomCardPrefab;

        private bool _initialized;

        private readonly List<RoomCard> _cards = new();

        public event Action pressedRefrash;
        public event Action<string> pressedJoin;
        public event Action<string, int, int> pressCreate;

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            _refrashButton.onClick.AddListener(OnClickRefrash);
            _createRoomButton.onClick.AddListener(OnClickRoomCreation);

            _initialized = true;
        }

        private void Start()
        {
            OnClickRefrash();
        }

        private void OnDestroy()
        {
            _refrashButton.onClick.RemoveListener(OnClickRefrash);
            _createRoomButton.onClick.RemoveListener(OnClickRoomCreation);
        }

        public void Display(RoomListResponse rooms)
        {
            Debug.Log($"Display {rooms.List.Length}");

            Clear();

            foreach (RoomsListElement room in rooms.List)
            {
                RoomCard card = Instantiate(_roomCardPrefab, _scrollOrigin);
                card.DisplayElement(room);
                card.pressJoined += OnPressJoin;
                
                _cards.Add(card);
            }
        }

        private void Clear()
        {
            foreach (RoomCard card in _cards)
            {
                card.pressJoined -= OnPressJoin;
                Destroy(card.gameObject);
            }

            _cards.Clear();
        }

        private void OnPressJoin(string uuid)
        {
            pressedJoin?.Invoke(uuid);
        }

        private void OnClickRoomCreation()
        {
            pressCreate?.Invoke(_roomNameField.text, _playersCap.value, _level.value);
        }

        private void OnClickRefrash()
        {
            pressedRefrash?.Invoke();
        }
    }
}