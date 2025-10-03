using UnityEngine;
using UnityEditor;


using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerEditor : Editor
{
    private EnemyManager manager;

    private void OnEnable()
    {
        manager = (EnemyManager)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyManager AI = (EnemyManager)target;
        if (GUILayout.Button("Spawn Random Ship"))
        {
            AI.SpawnShip();
        }
        
        if (manager.enemies == null || manager.enemies.Count == 0)
        {
            EditorGUILayout.HelpBox("Нет активных врагов.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Активные враги", EditorStyles.boldLabel);

        foreach (var kvp in manager.enemies)
        {
            Ship ship = kvp.ship;
            ShipArchetype archetype = kvp.archetype;

            if (ship == null || archetype == null) continue;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Ship name
            EditorGUILayout.LabelField(ship.name, GUILayout.Width(150));

            // Archetype
            EditorGUILayout.LabelField(archetype.type.ToString(), GUILayout.Width(100));

            // EnemyState color indicator
            Color stateColor = Color.gray;
            switch (archetype.state)
            {
                case EnemyState.Traveling: stateColor = Color.green; break;
                case EnemyState.Idle: stateColor = Color.cyan; break;
                case EnemyState.Retreating: stateColor = Color.red; break;
            }

            GUIStyle stateStyle = new GUIStyle(EditorStyles.boldLabel);
            stateStyle.normal.textColor = stateColor;
            EditorGUILayout.LabelField(archetype.state.ToString(), stateStyle, GUILayout.Width(100));

            // Direction arrow
            Vector2 dir = archetype.currentDirection.normalized;
            if (dir != Vector2.zero)
            {
                GUILayout.Label(GetArrowSymbol(dir), GUILayout.Width(30));
            }
            else
            {
                GUILayout.Label("-", GUILayout.Width(30));
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private string GetArrowSymbol(Vector2 dir)
    {
        if (Vector2.Dot(dir, Vector2.up) > 0.7f) return "↑";
        if (Vector2.Dot(dir, Vector2.down) > 0.7f) return "↓";
        if (Vector2.Dot(dir, Vector2.left) > 0.7f) return "←";
        if (Vector2.Dot(dir, Vector2.right) > 0.7f) return "→";
        return "↗";
    }
}
