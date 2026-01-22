using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    void Start()
    {
        if (PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.OnCoinChanged += Refresh;
            Refresh(PlayerWallet.Instance.Coin);
        }
    }

    void OnDestroy()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnCoinChanged -= Refresh;
    }

    void Refresh(int value)
    {
        if (coinText != null) coinText.text = value.ToString();
    }
}
