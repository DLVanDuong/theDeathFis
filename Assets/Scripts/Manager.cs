using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject statsPanel;
    public GameObject panelBag;
    public GameObject panelEquipment;
    public GameObject character;
    public GameObject over;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPanels();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPanels();
    }

    void FindPanels()
    {
        statsPanel = GameObject.FindGameObjectWithTag("StatsPanel");
        panelBag = GameObject.FindGameObjectWithTag("BagPanel");
        panelEquipment = GameObject.FindGameObjectWithTag("EquipmentPanel");
        character = GameObject.FindGameObjectWithTag("CharacterPanel");
        over = GameObject.FindGameObjectWithTag("OverPanel");
    }

    void Start()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (panelBag != null) panelBag.SetActive(false);
        if (panelEquipment != null) panelEquipment.SetActive(false);
        if (over != null) over.SetActive(true);

        if (character != null) character.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Đang mở Shop hoặc WeaponDetail thì không cho bật/tắt UI bằng phím
        if (ShopUIController.IsShopOpen) return;
        if (WeaponDetailUI.IsOpen) return;

        // C: giữ nguyên hành vi cũ (Stats + Bag + Equip)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleUIPanels();
            return;
        }

        // B: Bag + Equip (không Stats)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBagEquip();
            return;
        }

        // F: mở shop + bag (ShopUIController lo)
        if (Input.GetKeyDown(KeyCode.F))
        {
            var shop = FindAnyObjectByType<ShopUIController>();
            shop?.Open();
            return;
        }
    }

    // ===== CŨ: C mở Stats + Bag + Equip =====
    public void ToggleUIPanels()
    {
        if (statsPanel == null) return;

        bool newState = !statsPanel.activeSelf;

        // C chỉ Stats
        statsPanel.SetActive(newState);

        // tắt các panel khác
        if (panelBag != null) panelBag.SetActive(false);
        if (panelEquipment != null) panelEquipment.SetActive(false);

        if (over != null) over.SetActive(!newState);
        if (character != null) character.SetActive(!newState);

        ApplyCursorPause(newState);
    }

    // ===== MỚI: B mở Bag + Equip (không Stats) =====
    public void ToggleBagEquip()
    {
        if (panelBag == null) return;

        bool newState = !panelBag.activeSelf;

        if (panelBag != null) panelBag.SetActive(newState);
        if (panelEquipment != null) panelEquipment.SetActive(newState);

        // B không mở stats
        if (statsPanel != null) statsPanel.SetActive(false);

        if (over != null) over.SetActive(!newState);
        if (character != null) character.SetActive(!newState);

        ApplyCursorPause(newState);
    }

    private void ApplyCursorPause(bool uiOpen)
    {
        if (uiOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
