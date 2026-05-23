using Positron.Client.Mono;
using UnityEngine;

namespace Positron.Extras.HandTests.Object
{
    public class SimplyObjectSpawner : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;
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