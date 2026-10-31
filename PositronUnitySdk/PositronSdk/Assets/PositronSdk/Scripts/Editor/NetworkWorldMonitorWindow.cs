using Positron;
using Positron.Client.Room.MonitorData;
using UnityEditor;
using UnityEngine;

public class NetworkWorldMonitorWindow : EditorWindow
{
    private static WorldMonitorData CurrentData { get; set; }

    private Vector2 _scrollPosition;

    private bool _showClock = true;
    private bool _showGameObjects = true;
    private bool _showNetValues = true;
    private bool _showRpcs = true;
    private bool _wasEnsuredStartData;

    [MenuItem("Positron/Network/WorldMonitor")]
    public static void Open()
    {
        var window = GetWindow<NetworkWorldMonitorWindow>("World Monitor");
        window.minSize = new Vector2(350, 400);
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += Refrash;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Refrash;
        _wasEnsuredStartData = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("World Monitor Realtime Data", EditorStyles.boldLabel);

        if (!Application.isPlaying || PositronFacade.World == null)
        {
            EditorGUILayout.HelpBox("Run the game to see live network monitor data.", MessageType.Info);
            return;
        }

        if (!_wasEnsuredStartData)
        {
            CurrentData = PositronFacade.World.CollectMonitorData();
            _wasEnsuredStartData = true;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        _showClock = EditorGUILayout.BeginFoldoutHeaderGroup(_showClock, "Network Clock");

        if (_showClock)
        {
            EditorGUI.indentLevel++;
            NetworkClockData clock = CurrentData.ClockData;

            DrawDoubleField("Recent Server Time", clock.RecentServerTime, "s");
            DrawDoubleField("Client Time", clock.ClientTime, "s");
            DrawDoubleField("Time Scale", clock.TimeScale, "x");
            DrawDoubleField("Frame Offset", clock.FrameOffset, "f");

            GUI.contentColor = GetDriftColor(clock.Drift);
            DrawDoubleField("Drift (Server - Client)", clock.Drift, "s");
            GUI.contentColor = Color.white;
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        _showGameObjects = EditorGUILayout.BeginFoldoutHeaderGroup(_showGameObjects, "Game Objects Model");
        
        if (_showGameObjects)
        {
            EditorGUI.indentLevel++;
            GameObjectsModelData go = CurrentData.GameObjectsData;

            DrawIntField("Current On Scene", go.CurrentGameObjectsOnSceneCount);
            DrawIntField("Creation Deltas", go.CreationDeltaCount);
            DrawIntField("Move Deltas", go.MoveDeltaCount);
            DrawIntField("Destroy Deltas", go.DestroyDeltaCount);
            DrawIntField("Ownership Requests", go.OwnershipRequestDeltaCount);
            DrawIntField("Local Mapping Size", go.LocalCreationMappingSize);
            DrawIntField("Blacklist Size", go.LocalCreationBlackListSize);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        _showNetValues = EditorGUILayout.BeginFoldoutHeaderGroup(_showNetValues, "Net Values Model");

        if (_showNetValues)
        {
            EditorGUI.indentLevel++;
            NetValuesModelData nv = CurrentData.NetValuesData;

            DrawIntField("Add Delta Size", nv.CurrentAddDeltaSize);
            DrawIntField("Mod Delta Size", nv.CurrentModDeltaSize);
            DrawIntField("Outgoing Arena", nv.OutgoingArenaSize, "bytes");
            DrawIntField("Incoming Arena", nv.IncomingArenaSize, "bytes");
            DrawIntField("Memory Mapping Size", nv.DescriptedWorldMemoryMappingSize);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        _showRpcs = EditorGUILayout.BeginFoldoutHeaderGroup(_showRpcs, "RPCs Model");
        if (_showRpcs)
        {
            EditorGUI.indentLevel++;
            RpcsModelData rpcs = CurrentData.RpcsData;

            DrawIntField("Call Buffer Size", rpcs.CallBufferSize);
            DrawIntField("Delayed Calls Buffer", rpcs.DellaydCallsBufferSize);
            DrawIntField("RPC to Obj Mapping", rpcs.RpcToObjectMemoryMappingSize);
            DrawIntField("Outgoing Arena", rpcs.OutgoingArenaSize, "bytes");
            DrawIntField("Incoming Arena", rpcs.IncomingArenaSize, "bytes");

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.EndScrollView();
    }

    private void Refrash()
    {
        if (PositronFacade.World == null)
        {
            return;
        }

        CurrentData = PositronFacade.World.CollectMonitorData();
        Repaint();
    }

    private void DrawDoubleField(string label, double value, string suffix = "")
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(180));
        string valueText = string.IsNullOrEmpty(suffix) ? $"{value:F4}" : $"{value:F4} {suffix}";
        EditorGUILayout.LabelField(valueText);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawIntField(string label, int value, string suffix = "")
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(180));
        string valueText = string.IsNullOrEmpty(suffix) ? $"{value}" : $"{value} {suffix}";
        EditorGUILayout.LabelField(valueText);
        EditorGUILayout.EndHorizontal();
    }

    private Color GetDriftColor(double drift)
    {
        double absDrift = System.Math.Abs(drift);

        if (absDrift > 0.1)
        {
            return Color.red;
        }

        if (absDrift > 0.03)
        {
            return Color.yellow;
        }

        return Color.white;
    }
}