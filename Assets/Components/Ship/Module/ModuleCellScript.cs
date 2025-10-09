using System.Collections.Generic;
using UnityEngine;

public class ModuleCellScript : MonoBehaviour
{
    public SpriteRenderer mainSprite;
    public SpriteRenderer outfitSprite;
    public SpriteRenderer damageSprite;
    public List<Sprite> damageSprites;
    public ParticleSystem damagedParticles;

    public void Initialize(Sprite newMainSprite, Sprite newOutfitSprite,float alpha=1f)
    {
        mainSprite.sprite=newMainSprite;
        mainSprite.color = new Color(1f, 1f, 1f, alpha);
        outfitSprite.sprite=newOutfitSprite;
        outfitSprite.color = new Color(1f, 1f, 1f, alpha);
    }
    public void VizualizeDamage(bool damaged)
    {
        if (damaged)
        {
            damageSprite.sprite=damageSprites[Random.Range(0, damageSprites.Count)];
            damageSprite.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0,3)*90);
        }
        else damageSprite.sprite=null;
    }

}
