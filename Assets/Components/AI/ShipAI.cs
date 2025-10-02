using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    public ShipFactory SFactory;
    public List<GameObject> Ships;

    public void SpawnShip()
    {
        Vector2 screenMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 screenMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        
        float randomX = Random.Range(screenMin.x + screenMax.x * 0.15f, screenMax.x - screenMax.x * 0.15f);
        float y = screenMax.y * 1.3f;
        
        var newShip = SFactory.GetShip();
        newShip.transform.SetParent(this.transform);
        Ships.Add(newShip);
        
        newShip.transform.position = new Vector3(randomX, y, 0);
        var body = newShip.GetComponent<Ship>().inertialBody;
        body.mass = 1f;
        body.drag = 1f;
        body.maxSpeed = 10f;

        body.velocity = Vector2.down * Random.Range(0.3f, 4f);
    }
    void FixedUpdate()
    {
        foreach (var ship in Ships)
        {
            ship.GetComponent<Ship>().inertialBody.Tick(Time.deltaTime);
        }
    }

    private void Update()
    {
        
    }
}