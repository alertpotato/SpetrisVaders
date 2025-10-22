using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GameLoopSharedData))]
public class GameLoopRewardState : StateBehaviour
{
    public GameLoopSharedData Config;
    [SerializeField]private GameObject TraderPrefab;
    private GameObject currentTrader;
    public float moveDuration = 3f;
    public override void OnEnter()
    {
        currentTrader= Instantiate(TraderPrefab);
        currentTrader.transform.SetParent(transform);
        var item1 = ModuleFactory.Instance.GetModule();
        var item2 = ModuleFactory.Instance.GetModule();
        item1.layer = LayerMask.NameToLayer(GameLogic.Instance.environmentLayer);
        item2.layer = LayerMask.NameToLayer(GameLogic.Instance.environmentLayer);
        currentTrader.GetComponent<Trader>().UpdateShop(item1, item2);
        
        currentTrader.GetComponent<Trader>().button1.onClick.AddListener(() => RepairShip());
        currentTrader.GetComponent<Trader>().button2.onClick.AddListener(() => NextStage());
        
        Vector3 bottomRight = Config.MainCamera.ViewportToWorldPoint(new Vector3(0.7f, -0.5f, -Config.MainCamera.transform.position.z));
        Vector3 center = Config.MainCamera.ViewportToWorldPoint(new Vector3(0.7f, 0.5f, -Config.MainCamera.transform.position.z));
        StartCoroutine(MoveToPosition(currentTrader,bottomRight, center, moveDuration));
    }

    private void RepairShip()
    {
        foreach (var module in Config.playerShip.modules)
        {
            module.Repair();
        }
        foreach (var module in Config.playerShip.hullModules)
        {
            module.Repair();
        }
    }

    private void NextStage()
    {
        Config.GameLoopState.ChangeState<GameLoopEnemyWaveState>();
    }
    public override void OnExit()
    {
        Vector3 center = Config.MainCamera.ViewportToWorldPoint(new Vector3(0.7f, 0.5f, -Config.MainCamera.transform.position.z));
        Vector3 topOutside = Config.MainCamera.ViewportToWorldPoint(new Vector3(0.7f, 1.5f, -Config.MainCamera.transform.position.z));
        StartCoroutine(MoveToPosition(currentTrader,center, topOutside, moveDuration));
        Destroy(currentTrader,moveDuration+0.5f);
    }
    private IEnumerator MoveToPosition(GameObject g,Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            g.transform.position = Vector3.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        g.transform.position = to;
    }
}