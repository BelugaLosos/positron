using Positron.NetworkIoAPI;
using UnityEditor;

namespace Positron.Editor
{
    public class NetworkIoPoolMonitor : EditorWindow
    {
        private NetworkIoPoolStats _poolStats;
        private EditorWindow _window;

        [MenuItem("Positron/Network/IoPoolMonitor")]
        public static void Open()
        {
            NetworkIoPoolMonitor window = GetWindow<NetworkIoPoolMonitor>();
            window._window = window;
        }

        private void OnGUI()
        {
            if (PositronFacade.NetworkIoPool == null)
            {
                _window.Repaint();

                EditorGUILayout.LabelField("IoPool does not inited yet");
                return;
            }

            _poolStats = PositronFacade.NetworkIoPool.GetStats();

            EditorGUILayout.LabelField($"Writers poped: {_poolStats.WritersGetted}");
            EditorGUILayout.LabelField($"Writers puted: {_poolStats.WritersPutted}");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Readers poped: {_poolStats.ReadersGetted}");
            EditorGUILayout.LabelField($"Readers puted: {_poolStats.ReadersPutted}");

            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Writers active now: {_poolStats.ActiveWriters}");
            EditorGUILayout.LabelField($"Readers active now: {_poolStats.ActiveReaders}");

            _window.Repaint();
        }
    }
}