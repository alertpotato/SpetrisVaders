using TMPro;
using UnityEngine;

public class CursorTooltip : MonoBehaviour
{
    public float tooltipLife;
    public float lastUpdate;
    public TextMeshProUGUI tooltipText;
    public RectTransform tooltipRect;

    void Update()
    {
        if (lastUpdate + tooltipLife < Time.time) tooltipText.text = "";
    }
    public void UpdatePosition(Vector3 position)
    {
        tooltipRect.position = position;
    }
    public void UpdateTooltip(string tooltip,float lifeOverride=-1)
    {
        lastUpdate = lifeOverride!=-1 ? Time.time-(tooltipLife-lifeOverride) : Time.time;
        tooltipText.text = tooltip;
    }
}