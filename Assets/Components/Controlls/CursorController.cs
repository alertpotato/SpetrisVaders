using Unity.VisualScripting;
using UnityEngine;
public enum CursorMode {Circle, Triangle, Square, System}
[RequireComponent(typeof(LineRenderer))]
public class CursorController : MonoBehaviour
{
    [Header("Circle")]
    [SerializeField] private int segments = 64;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float pulseScale = 1.3f;
    [SerializeField] private float pulseSpeed = 5f;
    private float currentRadius;
    [Header("Line settings")]
    [SerializeField] private Color aimColor = Color.green;
    [SerializeField] private float startWidth = 0.1f;
    [SerializeField] private float endWidth = 0.1f;
    [SerializeField] Material lineMaterial;
    private Vector3 positionAdjastment;
    
    [Header("Components")]
    private LineRenderer line;
    [Header("Variables")]
    [SerializeField] private CursorMode currentMode = CursorMode.System;
    

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = false;
        line.startColor = aimColor;
        line.endColor = aimColor;
        line.sortingOrder = 70;
        line.startWidth = startWidth;
        line.endWidth = endWidth;
        line.material = lineMaterial;
        currentRadius = radius;
        DrawCircle(radius);
    }

    void Update()
    {
        switch (currentMode)
        {
            case CursorMode.System:
                DrawPlus();
                break;
            case CursorMode.Circle:
                currentRadius = Mathf.Lerp(currentRadius, radius, Time.deltaTime * 2f);
                DrawCircle(currentRadius);
                break;
            case CursorMode.Triangle:
                break;
            case CursorMode.Square:
                break;
        }
    }

    public void ChangeMode(CursorMode mode)
    {
        currentMode = mode;
    }
    public void AdjastPosition(Vector3 position)
    {
        positionAdjastment = position;
    }

    public void Pulse()
    {
        currentRadius = radius * pulseScale;
    }

    private void DrawCircle(float r)
    {
        float angleStep = 360f / segments;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            points[i] = new Vector3(Mathf.Cos(angle) * r + positionAdjastment.x, Mathf.Sin(angle) * r + positionAdjastment.y, 0);
        }
        line.positionCount = points.Length;
        line.SetPositions(points);
    }
    private void DrawPlus()
    {
        Vector3[] points = new Vector3[6];
        
        points[0] = new Vector3(-1 + positionAdjastment.x, positionAdjastment.y, 0);
        points[1] = new Vector3( 1 + positionAdjastment.x, positionAdjastment.y, 0);
        points[2] = new Vector3(positionAdjastment.x, positionAdjastment.y, 0);
        points[3] = new Vector3(positionAdjastment.x,  1 + positionAdjastment.y, 0);
        points[4] = new Vector3(positionAdjastment.x, -1 + positionAdjastment.y, 0);
        points[5] = new Vector3(positionAdjastment.x, positionAdjastment.y, 0);
        
        line.positionCount = points.Length;
        line.SetPositions(points);
    }
}