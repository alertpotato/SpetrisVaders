using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameGraphics", menuName = "Settings/Game Graphics")]
public class GameGraphics : ScriptableObject
{
    public static GameGraphics Instance { get; private set; }
    [Header("Colors")]
    [ColorUsage(true, true)]
    public Color mainHudColor;
    [ColorUsage(true, true)]
    public Color enemyColor;

    [Header("Materials")]
    public Material simpleHDRColor;

    [Header("Particle Effects")]
    public List<ParticleSystem> effects;
    
    private void OnEnable()
    {
        Instance = this;
    }
}