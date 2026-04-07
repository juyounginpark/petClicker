using UnityEngine;

/// <summary>
/// C2: 치명타 확률 +10%
/// </summary>
public class B2 : ItemEffect
{
    private float critBonus = 0.07f;

    public override void Activate()
    {
        if (click != null)
            click.AddCritChance(critBonus);
    }

    public override void Deactivate()
    {
        if (click != null)
            click.AddCritChance(-critBonus);
    }
}
