using UnityEngine;

public class ModuleCellScript : MonoBehaviour
{
    public SpriteRenderer mainSprite;
    public SpriteRenderer outfitSprite;

    public void Initialize(Sprite newMainSprite, Sprite newOutfitSprite,float alpha=1f)
    {
        mainSprite.sprite=newMainSprite;
        mainSprite.color = new Color(1f, 1f, 1f, alpha);
        outfitSprite.sprite=newOutfitSprite;
        outfitSprite.color = new Color(1f, 1f, 1f, alpha);
    }
}
