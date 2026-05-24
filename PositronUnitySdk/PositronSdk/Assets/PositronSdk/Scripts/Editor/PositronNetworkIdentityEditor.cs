namespace Positron.Editor
{
    using Positron.Client.Mono;
    using UnityEditor;

    [CustomEditor(typeof(PositronNetworkIdentity))]
    public class PositronNetworkIdentityEditor : Editor
    {
        private PositronNetworkIdentity _object;

        private void OnEnable()
        {
            _object = (PositronNetworkIdentity)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(25);

            if (!_object.IsFullyInitialized)
            {
                EditorGUILayout.LabelField("Object is not fully initialized !!!");
                EditorGUILayout.Space(25);
            }

            EditorGUILayout.LabelField($"Object ID: {_object.ObjectId}");
            EditorGUILayout.LabelField($"Sub object ID: {_object.SubObjectId} ({(_object.SubObjectId == 0 ? "Not SUB object" : "Is SUB object")})");

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField($"Owner ID: {_object.OwnerClientId}");

            EditorGUILayout.Space(15);

            if (PositronFacade.World == null || !PositronFacade.World.InRoom)
            {
                EditorGUILayout.LabelField("Can`t show ownership data outside of room!");
            }
            else
            {
                EditorGUILayout.LabelField($"Is Mine: {_object.IsMine}");
                EditorGUILayout.LabelField($"Host is Owner: {_object.IsHost} (Host ID: {PositronFacade.World.HostId})");
            }
        }
    }
}