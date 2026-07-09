using Positron.Client.Mono;
using UnityEngine;

namespace Positron.Extras.HandTests.Object
{
    public class SimplyObjectManipulationDemo : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField][Min(0.001f)] private float _rotSensitivity = 10f;
        [SerializeField] private PositronNetworkIdentity _prefab;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _groundMask))
                {
                    PositronFacade.World.SpawnObject(_prefab, hit.point, Quaternion.identity);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                PositronNetworkIdentity obj = Hitscan();
                PositronFacade.World.Destroy(obj);
            }

            if (Input.GetMouseButton(2))
            {
                PositronNetworkIdentity obj = Hitscan();
                
                if (obj != null)
                {
                    obj.transform.Translate(Input.mousePositionDelta * 50 * Time.deltaTime);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PositronFacade.LeaveRoom();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                PositronNetworkIdentity obj = Hitscan();

                if (obj != null)
                {
                    PositronFacade.World.RequestOwnershipOn(obj);
                }
            }

            if (Input.GetKey(KeyCode.X))
            {
                ProcessRotationByAxis(Vector3.right);
            }

            if (Input.GetKey(KeyCode.Y))
            {
                ProcessRotationByAxis(Vector3.up);
            }

            if (Input.GetKey(KeyCode.Z))
            {
                ProcessRotationByAxis(Vector3.forward);
            }
        }

        private void ProcessRotationByAxis(Vector3 rotAxis)
        {
            float mouseWheelDelta = Input.mouseScrollDelta.y + Input.mouseScrollDelta.y;
            PositronNetworkIdentity obj = Hitscan();

            if (obj != null)
            {
                obj.transform.Rotate(rotAxis, mouseWheelDelta * _rotSensitivity * Time.deltaTime);
            }
        }

        private PositronNetworkIdentity Hitscan()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _groundMask))
            {
                return hit.collider.GetComponent<PositronNetworkIdentity>();
            }

            return null;
        }
    }
}