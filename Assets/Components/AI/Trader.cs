using UnityEngine;
using UnityEngine.UI;

class Trader : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] GameObject space1;
    [SerializeField] GameObject space2;
    public Button button1;
    public Button button2;

    public void UpdateShop(GameObject item1, GameObject item2)
    {
        item1.transform.SetParent(space1.transform);
        item1.transform.localPosition = Vector3.zero;
        item2.transform.SetParent(space2.transform);
        item2.transform.localPosition = Vector3.zero;
    }
}