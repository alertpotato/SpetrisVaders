using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(DockingVisualizer))]
public class DockingVisualizerEditor : Editor
{
    private object candidatesManager;
    private MethodInfo getCandidatesMethod;

    private void OnEnable()
    {
        var vis = (DockingVisualizer)target;

        var field = typeof(DockingVisualizer).GetField("candidates", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
        {
            candidatesManager = field.GetValue(vis);
            if (candidatesManager != null)
            {
                getCandidatesMethod = candidatesManager.GetType().GetMethod("GetCandidatesInOrder", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (candidatesManager == null || getCandidatesMethod == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Docking Candidates (FIFO)", EditorStyles.boldLabel);

        var list = getCandidatesMethod.Invoke(candidatesManager, null) as System.Collections.IEnumerable;

        if (list != null)
        {
            int i = 0;
            foreach (var cand in list)
            {
                var moduleField = cand.GetType().GetField("module", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var anchorField = cand.GetType().GetField("anchor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                GameObject module = moduleField?.GetValue(cand) as GameObject;
                var anchor = (Vector2Int)(anchorField?.GetValue(cand) ?? Vector2Int.zero);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"#{i}", GUILayout.Width(30));

                    if (module != null)
                        EditorGUILayout.ObjectField(module, typeof(GameObject), true);
                    else
                        EditorGUILayout.LabelField("null");

                    EditorGUILayout.LabelField($"Anchor: {anchor}");
                }
                i++;
            }

            if (i == 0)
                EditorGUILayout.HelpBox("No candidates in list", MessageType.Info);
        }
    }
}
