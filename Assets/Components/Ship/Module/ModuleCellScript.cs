using UnityEngine;

public class ModuleCellScript : MonoBehaviour
{
    public SpriteRenderer mainSprite;
    public SpriteRenderer outfitSprite;

    public void Initialize(Sprite newMainSprite, Sprite newOutfitSprite)
    {
        mainSprite.sprite=newMainSprite;
        outfitSprite.sprite=newOutfitSprite;
    }
}
