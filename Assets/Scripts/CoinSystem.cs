using UnityEngine;
using TMPro;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private int currentCoins;
    private float coinBonusRate;

    private void Start()
    {
        currentCoins = 0;
        coinBonusRate = 0f;
        UpdateCoinText();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinText();
    }

    public void AddCoinBonusRate(float rate)
    {
        coinBonusRate += rate;
    }

    public void RemoveCoinBonusRate(float rate)
    {
        coinBonusRate -= rate;
        if (coinBonusRate < 0f) coinBonusRate = 0f;
    }

    public int GetCoins() => currentCoins;
    public float GetCoinBonusRate() => coinBonusRate;

    private void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = $"C{currentCoins}";
    }
}
