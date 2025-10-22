using UnityEngine;

public class LootBox : MonoBehaviour
{
    [SerializeField] private int creditReward=0;

    public void SetReward(int credit)
    {
        creditReward = credit;
    }

    public int GetReward()
    {
        return creditReward;
    }
}