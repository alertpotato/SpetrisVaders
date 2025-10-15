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

    public void ActivateScan(Vector3 scannedTargetPos,Vector2 dimensionMin, Vector2 dimensionMax,string shipType)
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
        
        text.text = "Type: "+shipType.ToUpper();
        text.rectTransform.sizeDelta = new Vector2(size.x-10,20);
    }

    public void DeactivateScan()
    {
        Radar.gameObject.SetActive(false);
    }
}