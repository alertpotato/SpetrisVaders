using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyScan : MonoBehaviour
{
    public GameObject Radar;
    public RectTransform borderRect;
    public TextMeshProUGUI text;
    private float pixelPerUnit = 12f;
    private Camera cam;

    public void Initialize(Camera mainCamera)
    {
        cam = mainCamera;
        Radar.gameObject.SetActive(true);
    }

    public void ActivateScan(Ship ship,Vector3 scannedTargetPos,Vector2 dimensionMin, Vector2 dimensionMax,string shipType)
    {
        Radar.gameObject.SetActive(true);
        Radar.transform.position = cam.WorldToScreenPoint(scannedTargetPos);
        Vector2 size = new Vector2(
            Mathf.Abs(dimensionMax.x - dimensionMin.x) * pixelPerUnit + 30,
            Mathf.Abs(dimensionMax.y - dimensionMin.y) * pixelPerUnit + 30
        );
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            borderRect.parent as RectTransform,
            cam.WorldToScreenPoint(scannedTargetPos),
            cam,
            out Vector2 localPos);
        borderRect.anchoredPosition = localPos;
        
        borderRect.sizeDelta = size;
        
        text.text = "T : "+shipType.ToUpper()+"\n"+ShipInformnation(ship);
        //text.rectTransform.sizeDelta = new Vector2(size.x-10,20);
    }

    public void DeactivateScan()
    {
        Radar.gameObject.SetActive(false);
    }

    public string ShipInformnation(Ship ship)
    {
        var countedTypes = new HashSet<ModuleType>
        {
            ModuleType.Canon,
            ModuleType.Missile,
            ModuleType.PointDefense
        };

        var countM = ship.modules
            .Where(m => m != null && countedTypes.Contains(m.data.type))
            .GroupBy(m => m.data.type)
            .ToDictionary(g => g.Key, g => g.Count());
        string info = "";
        foreach (var i in countM)
        {
            if (i.Key==ModuleType.Canon) info+="CN : "+i.Value+"\n";
            if (i.Key==ModuleType.Missile) info+="MS : "+i.Value+"\n";
            if (i.Key==ModuleType.PointDefense) info+="PD : "+i.Value+"\n";
        }

        info +="V : "+ ship.inertialBody.velocity;
        return info;
    }
}