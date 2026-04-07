using UnityEngine;

/// <summary>
/// C2: 치명타 확률 +10%
/// </summary>
public class A2 : ItemEffect
{
    private float critBonus = 0.1f;

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
