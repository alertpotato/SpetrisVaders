using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModuleFactory))]
public class ModuleFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ModuleFactory factory = (ModuleFactory)target;
        if (GUILayout.Button("Spawn Random Module"))
        {
            factory.SpawnRandomModule();
        }
    }
}