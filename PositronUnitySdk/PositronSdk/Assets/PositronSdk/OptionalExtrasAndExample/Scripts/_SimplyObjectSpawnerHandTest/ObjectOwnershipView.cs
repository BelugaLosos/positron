using Positron.Client.Mono;
using UnityEngine;

namespace Positron.Extras.HandTests
{
    [RequireComponent(typeof(PositronNetworkIdentity))]
    public class ObjectOwnershipView : MonoBehaviour
    {
        [SerializeField] private Renderer _targetRenderer;

        [SerializeField] private Color _myColorl = Color.green;
        [SerializeField] private Color _foreignColor = Color.red;

        private PositronNetworkIdentity _identity;

        private void Awake()
        {
            _identity = GetComponent<PositronNetworkIdentity>();
            _identity.completeInitWithEmptyCallback += DisplayOwnership;
            _identity.transferedWithEmptyCallback += DisplayOwnership;
        }

        private void OnDestroy()
        {
            _identity.completeInitWithEmptyCallback -= DisplayOwnership;
            _identity.transferedWithEmptyCallback -= DisplayOwnership;
        }

        private void DisplayOwnership()
        {
            _targetRenderer.material.color = _identity.IsMine ? _myColorl : _foreignColor;
        }
    }
}