using UnityEngine;

/// <summary>
/// C3: 코인 획득량 +10%
/// </summary>
public class A3 : ItemEffect
{
    private float coinBonus = 0.20f;

    public override void Activate()
    {
        if (coinSystem != null)
            coinSystem.AddCoinBonusRate(coinBonus);
    }

    public override void Deactivate()
    {
        if (coinSystem != null)
            coinSystem.RemoveCoinBonusRate(coinBonus);
    }
}
