using Positron.Client.DataTransferObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Positron.Demo.RoomsBrowser.Views
{
    public sealed class RoomCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _palyersCountText;
        [SerializeField] private Button _joinButton;

        private string _uuid;

        public event Action<string> pressJoined;

        private void Awake()
        {
            _joinButton.onClick.AddListener(OnClickJoin);
        }

        private void OnDestroy()
        {
            _joinButton.onClick.RemoveListener(OnClickJoin);
        }

        public void DisplayElement(RoomsListElement element)
        {
            _nameText.text = element.Name;
            _palyersCountText.text = $"{element.CurrentPlayers}/{element.MaxPlayers}";
            _uuid = element.Uuid;
        }

        private void OnClickJoin()
        {
            pressJoined?.Invoke(_uuid);
        }
    }
}