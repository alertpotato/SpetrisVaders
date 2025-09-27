using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ShipGrid))]
public class ShipGridEditor : Editor
{
    private const int cellSize = 20;
    private Vector2 scrollPos;

    private static readonly Dictionary<ModuleType, Color> typeColors = new()
    {
        { ModuleType.Canon,  new Color(0.8f, 0.3f, 0.8f) },
        { ModuleType.Missile, new Color(0.9f, 0.2f, 0.2f) },
        { ModuleType.PointDefense, new Color(0.2f, 0.9f, 0.2f) },
        { ModuleType.Shield, new Color(0.2f, 0.4f, 0.9f) },
        { ModuleType.Speed,  new Color(0.9f, 0.9f, 0.2f) },
        { ModuleType.Empty,  Color.gray }
    };

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShipGrid shipGrid = (ShipGrid)target;

        GUILayout.Space(10);
        GUILayout.Label("Ship Grid Preview", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

        Rect rect = GUILayoutUtility.GetRect(400, 400);
        GUI.Box(rect, GUIContent.none);

        Handles.BeginGUI();

        Vector2 center = rect.center;

        foreach (var kvp in shipGrid.grid)
        {
            Vector2Int pos = kvp.Key;
            ShipModule module = kvp.Value;

            if (module == null || module.data == null) 
                continue;
            
            Color col = typeColors.TryGetValue(module.data.type, out var c) ? c : Color.white;

            Vector2 drawPos = center + new Vector2(pos.x * cellSize, -pos.y * cellSize);
            Rect cellRect = new Rect(drawPos.x - cellSize/2, drawPos.y - cellSize/2, cellSize, cellSize);

            EditorGUI.DrawRect(cellRect, col);

            Handles.color = Color.black;
            Handles.DrawAAPolyLine(2f,
                new Vector3(cellRect.xMin, cellRect.yMin),
                new Vector3(cellRect.xMax, cellRect.yMin),
                new Vector3(cellRect.xMax, cellRect.yMax),
                new Vector3(cellRect.xMin, cellRect.yMax),
                new Vector3(cellRect.xMin, cellRect.yMin)
            );
        }

        Handles.EndGUI();
        EditorGUILayout.EndScrollView();
    }
}
