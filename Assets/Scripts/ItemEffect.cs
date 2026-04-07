using UnityEngine;

/// <summary>
/// 아이템 효과 베이스 클래스. 슬롯에 장착 시 Activate, 해제 시 Deactivate 호출.
/// </summary>
public abstract class ItemEffect : MonoBehaviour
{
    protected Click click;
    protected CoinSystem coinSystem;

    public void Init(Click click, CoinSystem coinSystem)
    {
        this.click = click;
        this.coinSystem = coinSystem;
    }

    public abstract void Activate();
    public abstract void Deactivate();
}
