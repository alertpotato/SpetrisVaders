using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ShipFactory : MonoBehaviour
{
    public GameObject shipPrefab;
    public ModuleFactory modules;
    public int shipCount;
    
    void Awake()
    {
        shipCount = 0;
    }

    public GameObject GetShip(int shipAlignment=180,Faction faction = Faction.Neutral)
    {
        shipCount++;
        var offCameraPoint = new Vector3(-999, -999, 0);
        GameObject ship = Instantiate(shipPrefab, offCameraPoint, Quaternion.identity, this.transform);
        var ShipScript = ship.GetComponent<Ship>();
        ShipScript.shipAlignment = shipAlignment;
        ShipScript.InitializeShip(faction);
        var cockpit = modules.GetCockpitModule(shipAlignment);
        RandomAttach(ShipScript,out GameObject module,cockpit);
        ship.name = $"Ship_{shipCount}";
        if (faction == Faction.Player) ship.layer = LayerMask.NameToLayer(GameLogic.Instance.playerLayer);
        else ship.layer = LayerMask.NameToLayer(GameLogic.Instance.enemyLayer);
        cockpit.layer = ship.layer;
        return ship;
    }
    public GameObject GetShip(Dictionary<ModuleType, int> moduleWeights = null,int numberOfModules = 3,GameObject predefinedShipPrefab = null,Faction faction = Faction.EvilFleet,int shipAlignment=180,Dictionary<Vector2Int, float> directionChances=null)
    {
        int failStatePreventor =  numberOfModules*50;
        int counter = 0;
        GameObject ship;
        if (predefinedShipPrefab != null) ship = predefinedShipPrefab;
            else ship = GetShip(shipAlignment,faction);
        var ShipScript = ship.GetComponent<Ship>();
        if (faction == Faction.Player) ship.layer = LayerMask.NameToLayer(GameLogic.Instance.playerLayer);;
        ShipScript.InitializeShip(faction);
        
        while (ShipScript.modules.Count < numberOfModules+1)
        {
            counter++;
            if (counter >= failStatePreventor)
            {
                Debug.LogWarning($"Failed to create ship with {failStatePreventor} itrerations");
                break;
            }

            if (!RandomAttach(ShipScript, out GameObject module, moduleWeights: moduleWeights,directionChances:directionChances))
            {
                Destroy(module);
            }
        }

        string m = "";
        foreach (var module in ShipScript.modules)
        {
            if (module.data.type== ModuleType.Cockpit) continue;
            m += module.data.moduleName[0];
        }
        ship.name = $"Ship_{m}_{shipCount}";
        return ship;
    }
    public bool RandomAttach(
        Ship ship,
        out GameObject newModule,
        GameObject moduleToAttach = null,
        Dictionary<ModuleType, int> moduleWeights = null,
        Dictionary<Vector2Int, float> directionChances = null)
    {
        bool attached = false;
        newModule = moduleToAttach == null ? modules.GetModule(moduleWeights: moduleWeights) : moduleToAttach;
        var module = newModule.GetComponent<ShipModule>();

        var borderEmptyCells = ship.grid.GetBorderEmptyCells().ToList();
        if (borderEmptyCells.Count == 0)
            return false;

        // Центр корабля (обычно 0,0)
        Vector2 center = Vector2.zero;

        // 1️⃣ Берем направление по шансам
        Vector2Int chosenDir = RollDirection(directionChances);

        // 2️⃣ Считаем "оценку привлекательности" каждой клетки
        // Чем ближе к центру и чем больше она в выбранном направлении — тем выше приоритет
        var scoredCells = borderEmptyCells
            .Select(cell => new
            {
                Cell = cell,
                Score = ComputeDirectionalScore(cell, center, chosenDir)
            })
            .OrderByDescending(c => c.Score)
            .ToList();

        // 3️⃣ Пробуем прикрепить начиная с лучших по Score
        foreach (var candidate in scoredCells)
        {
            var anchor = candidate.Cell;
            if (ship.grid.TryGetAttachPosition(module, anchor, out var attachAdjustment, module.currentRotation))
            {
                var anchorList = new List<AnchorOption> { new(anchor, attachAdjustment) };
                var moduleCandidate = new Candidate(newModule, anchorList);
                ship.AttachModule(moduleCandidate);
                attached = true;
                break;
            }
        }

        return attached;
    }

    /// Роллит направление по весам
    private Vector2Int RollDirection(Dictionary<Vector2Int, float> chances)
    {
        if (chances == null || chances.Count == 0)
        {
            chances = new()
            {
                { Vector2Int.up, 0.25f },
                { Vector2Int.down, 0.25f },
                { Vector2Int.left, 0.25f },
                { Vector2Int.right, 0.25f }
            };
        }

        float total = chances.Values.Sum();
        float roll = Random.value * total;
        float cumulative = 0f;

        foreach (var kvp in chances)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return Vector2Int.up;
    }

    /// Функция оценки клетки по направлению и расстоянию к центру
    private float ComputeDirectionalScore(Vector2Int cell, Vector2 center, Vector2Int direction)
    {
        // Чем ближе к центру — тем лучше
        float distanceScore = 1f / (1f + Vector2.Distance(cell, center));

        // Проекция на выбранное направление — насколько клетка “в ту сторону”
        float directionalBias = Vector2.Dot(((Vector2)cell).normalized, ((Vector2)direction).normalized);
        directionalBias = Mathf.Clamp01((directionalBias + 1f) / 2f); // 0–1

        // Финальный вес: комбинация направления и близости
        return distanceScore * 0.7f + directionalBias * 0.3f;
    }
}