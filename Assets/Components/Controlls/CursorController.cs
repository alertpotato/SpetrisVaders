using Unity.VisualScripting;
using UnityEngine;
public enum CursorMode {Circle, Triangle, Square, System}
[RequireComponent(typeof(LineRenderer))]
public class CursorController : MonoBehaviour
{
    [Header("Circle")]
    [SerializeField] private int segments = 64;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float pulseScale = 1.4f;
    [SerializeField] private float pulseSpeed = 5f;
    private float currentRadius;
    [Header("Line settings")]
    [SerializeField] private Color aimColor = Color.green;
    [SerializeField] private float startWidth = 0.1f;
    [SerializeField] private float endWidth = 0.1f;
    [SerializeField] Material lineMaterial;
    
    
    [Header("Components")]
    private LineRenderer line;
    [Header("Variables")]
    [SerializeField] private CursorMode currentMode = CursorMode.System;
    private Vector3 positionAdjastment = Vector3.zero;
    private Vector2 currentDirection = Vector2.zero;

    [SerializeField] private GameObject PDCursor;
    [SerializeField] private ParticleSystem PDEffectLeft;
    [SerializeField] private ParticleSystem PDEffectRight;
    

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
                PDCursor.transform.localPosition = positionAdjastment;
                PDCursor.transform.up = currentDirection;
                break;
        }
    }

    public void ChangeMode(CursorMode mode)
    {
        if (mode == CursorMode.Square)
        {
            PDCursor.SetActive(true);
            line.enabled = false;
        }
        else
        {
            PDCursor.SetActive(false);
            line.enabled = true;
        }
        currentMode = mode;
    }
    public void AdjastPosition(Vector3 position, Vector2 direction)
    {
        positionAdjastment = position;
        currentDirection = direction;
    }

    public void Pulse()
    {
        switch (currentMode)
        {
            case CursorMode.System:
                break;
            case CursorMode.Circle:
                currentRadius = radius * pulseScale;
                break;
            case CursorMode.Triangle:
                break;
            case CursorMode.Square:
                PDEffectLeft.Play();
                PDEffectRight.Play();
                break;
        }
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
    private void DrawSquare()
    {
        Vector3[] points = new Vector3[4];
        
        points[0] = new Vector3(-1 + positionAdjastment.x, 1 + positionAdjastment.y, 0);
        points[1] = new Vector3( 1 + positionAdjastment.x, 1 + positionAdjastment.y, 0);
        points[2] = new Vector3( 1 + positionAdjastment.x,-1 + positionAdjastment.y, 0);
        points[3] = new Vector3(-1 + positionAdjastment.x,-1 + positionAdjastment.y, 0);
        
        line.positionCount = points.Length;
        line.SetPositions(points);
    }
}