using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int coin = 0;
    public int Coin => coin;

    public event Action<int> OnCoinChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Nếu Player của bạn đã DontDestroyOnLoad ở chỗ khác thì bỏ dòng dưới
        // DontDestroyOnLoad(gameObject);
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        coin += amount;
        OnCoinChanged?.Invoke(coin);
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0) return true;
        if (coin < amount) return false;
        coin -= amount;
        OnCoinChanged?.Invoke(coin);
        return true;
    }

    public void SetCoin(int value)
    {
        coin = Mathf.Max(0, value);
        OnCoinChanged?.Invoke(coin);
    }
}
