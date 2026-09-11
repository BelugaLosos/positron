using MessagePack;
using Positron.Client.Mono;
using Positron.Client.Mono.Interfaces;
using Positron.Client.NetValues;
using Positron.Client.NetValues.Attributes;
using Positron.Client.NetValues.Implements;
using TMPro;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    public partial class ObjectValueTest : MonoBehaviour, INetworkAwakeble, INetworkDestructable
    {
        [SerializeField] private TextMeshProUGUI _displayText;

        [Networked(NetValueAuthority.Owner)] private NetValueComplex<ObjectValueTestData> _someValue = new();
        [Networked(NetValueAuthority.Server, true)] private NetValueComplex<ObjectValueTestData> _anotherV;

        public void OnNetworkAwake()
        {
            _someValue.changed += DisplayCurrentValue;
            DisplayCurrentValue();
        }

        public void OnNetworkDestroy()
        {
            _someValue.changed -= DisplayCurrentValue;
        }

        private void DisplayCurrentValue()
        {
            _displayText.text = _someValue.Value.IntValue.ToString();
        }

        public void PutRandom()
        {
            ObjectValueTestData data = _someValue.Value;
            data.IntValue = Random.Range(0, 1000);
            _someValue.Value = data;
        }
    }

    [MessagePackObject]
    public struct ObjectValueTestData
    {
        [Key(0)] public int IntValue { get; set; }
    }
}