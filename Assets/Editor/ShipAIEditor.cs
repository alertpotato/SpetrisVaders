using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShipAI))]
public class ShipAIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ShipAI AI = (ShipAI)target;
        if (GUILayout.Button("Spawn Random Ship"))
        {
            AI.SpawnShip();
        }
    }
}