using MessagePack;
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
        [Networked(NetValueAuthority.Owner)] private IntNetValue _intV = new();
        [Networked(NetValueAuthority.Owner)] private UintNetValue _uintV = new();
        [Networked(NetValueAuthority.Owner)] private ShortNetValue _shortV = new();
        [Networked(NetValueAuthority.Owner)] private LongNetValue _longV = new();
        [Networked(NetValueAuthority.Owner)] private UlongNetValue _ulongV = new();
        [Networked(NetValueAuthority.Owner)] private Vector2NetValue _vec2V = new();
        [Networked(NetValueAuthority.Owner)] private Vector3NetValue _vec3V = new();
        [Networked(NetValueAuthority.Owner)] private Vector4NetValue _vec4V = new();
        [Networked(NetValueAuthority.Owner)] private QuaternionNetValue _quatV = new();
        [Networked(NetValueAuthority.Owner)] private Vector2IntNetValue _vec2IntV = new();
        [Networked(NetValueAuthority.Owner)] private Vector3IntNetValue _vec3IntV = new();
        [Networked(NetValueAuthority.Owner)] private FloatNetValue _floatV = new();
        [Networked(NetValueAuthority.Owner)] private DoubleNetValue _doubleV = new();
        [Networked(NetValueAuthority.Owner)] private ByteNetValue _byteV = new();
        [Networked(NetValueAuthority.Owner)] private SbyteNetValue _sbyteV = new();
        [Networked(NetValueAuthority.Owner)] private BoolNetValue _boolV = new();

        private const int VAL_INT = -(int.MaxValue / 3);
        private const int VAL_INT_1 = int.MinValue / 2;
        private const uint VAL_UINT = uint.MaxValue / 2; 
        private const short VAL_SHORT = short.MinValue / 2;
        private const long VAL_LONG = long.MinValue / 2;
        private const ulong VAL_ULONG = ulong.MinValue / 2;
        private readonly Vector2 VAL_VEC2 = new(float.MinValue / 2f, float.MaxValue / 2f);
        private readonly Vector3 VAL_VEC3 = new(float.MinValue / 2f, float.MaxValue / 2f, float.MinValue / 2f);
        private readonly Vector4 VAL_VEC4 = new(float.MinValue / 2f, float.MaxValue / 2f, float.MinValue / 2f, float.MaxValue / 2f);
        private readonly Quaternion VAL_QUAT = new(0.545454f, 0.1223f, 0.18766778f, 45f);
        private readonly Vector2Int VAL_VEC2_INT = new(int.MinValue / 2, int.MaxValue / 3);
        private readonly Vector3Int VAL_VEC3_INT = new(int.MinValue / 2, int.MaxValue / 3, int.MinValue / 2);
        private const float VAL_FLOAT = float.MinValue / 2.14656f;
        private const double VAL_DOUBLE = double.MinValue / 1.53466;
        private const byte VAL_BYTE = 123;
        private const sbyte VAL_SBYTE = -101;
        private const bool VAL_BOOL = true;    

        public void OnNetworkAwake()
        {
            _boolV.changed += OnChange;
            OnChange();
        }

        public void OnNetworkDestroy()
        {
            _boolV.changed -= OnChange;
        }

        public void Call()
        {
            _someValue.Value = new(VAL_INT);
            _intV.Value = VAL_INT_1;
            _uintV.Value = VAL_UINT;
            _shortV.Value = VAL_SHORT;
            _longV.Value = VAL_LONG;
            _ulongV.Value = VAL_ULONG;
            _vec2V.Value = VAL_VEC2;
            _vec3V.Value = VAL_VEC3;
            _vec4V.Value = VAL_VEC4;
            _quatV.Value = VAL_QUAT;
            _vec2IntV.Value = VAL_VEC2_INT;
            _vec3IntV.Value = VAL_VEC3_INT;
            _floatV.Value = VAL_FLOAT;
            _doubleV.Value = VAL_DOUBLE;
            _byteV.Value = VAL_BYTE;
            _sbyteV.Value = VAL_SBYTE;
            _boolV.Value = VAL_BOOL;
        }

        private void OnChange()
        {
            bool passed = true;

            if(_someValue.Value.IntValue != VAL_INT)
            {
                Debug.LogError($"some v {_someValue.Value.IntValue} != {VAL_INT}");
                passed = false;
            }

            if (_intV.Value != VAL_INT_1) 
            {
                Debug.LogError($"int v {_intV.Value} != {VAL_INT_1}");
                passed = false;
            }

            if (_uintV.Value != VAL_UINT)
            {
                Debug.LogError($"uint v {_uintV.Value} != {VAL_UINT}");
                passed = false;
            }

            if (_shortV.Value != VAL_SHORT)
            {
                Debug.LogError($"short v {_shortV.Value} != {VAL_SHORT}");
                passed = false;
            }

            if (_longV.Value != VAL_LONG)
            {
                Debug.LogError($"long v {_longV.Value} != {VAL_LONG}");
                passed = false;
            }

            if (_ulongV.Value != VAL_ULONG)
            {
                Debug.LogError($"ulong v {_ulongV.Value} != {VAL_ULONG}");
                passed = false;
            }

            if (_vec2V.Value != VAL_VEC2)
            {
                Debug.LogError($"vec2 v {_vec2V.Value} != {VAL_VEC2}");
                passed = false;
            }

            if (_vec3V.Value != VAL_VEC3)
            {
                Debug.LogError($"vec3 v {_vec3V.Value} != {VAL_VEC3}");
                passed = false;
            }

            if (_vec4V.Value != VAL_VEC4)
            {
                Debug.LogError($"vec4 v {_vec4V.Value} != {VAL_VEC4}");
                passed = false;
            }

            if (_quatV.Value != VAL_QUAT)
            {
                Debug.LogError($"quat v {_quatV.Value} != {VAL_QUAT}");
                passed = false;
            }

            if (_vec2IntV.Value != VAL_VEC2_INT)
            {
                Debug.LogError($"vec2Int v {_vec2IntV.Value} != {VAL_VEC2_INT}");
                passed = false;
            }

            if (_vec3IntV.Value != VAL_VEC3_INT)
            {
                Debug.LogError($"vec3Int v {_vec3IntV.Value} != {VAL_VEC3_INT}");
                passed = false;
            }

            if (_floatV.Value != VAL_FLOAT)
            {
                Debug.LogError($"float v {_floatV.Value} != {VAL_FLOAT}");
                passed = false;
            }

            if (_doubleV.Value != VAL_DOUBLE)
            {
                Debug.LogError($"double v {_doubleV.Value} != {VAL_DOUBLE}");
                passed = false;
            }

            if (_byteV.Value != VAL_BYTE)
            {
                Debug.LogError($"byte v {_byteV.Value} != {VAL_BYTE}");
                passed = false;
            }

            if (_sbyteV.Value != VAL_SBYTE)
            {
                Debug.LogError($"sbyte v {_sbyteV.Value} {VAL_SBYTE}");
                passed = false;
            }

            if (_boolV.Value != VAL_BOOL)
            {
                Debug.LogError($"bool v {_boolV.Value} != {VAL_BOOL}");
                passed = false;
            }

            _displayText.text = passed.ToString();
        }
    }

    [MessagePackObject]
    public struct ObjectValueTestData
    {
        [Key(0)] public int IntValue { get; set; }

        public ObjectValueTestData(int value) 
        { 
            IntValue = value;
        }
    }
}