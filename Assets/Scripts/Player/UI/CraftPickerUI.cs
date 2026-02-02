using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftPickerUI : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public Button closeBtn;

    [Header("Item Prefab")]
    public PickerItemButton itemPrefab;

    private readonly List<PickerItemButton> spawned = new();

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (closeBtn) closeBtn.onClick.AddListener(Close);
    }

    public void Open<T>(List<T> list, Func<T, Sprite> getIcon, Func<T, string> getName, Action<T> onPick)
    {
        if (panel == null)
        {
            Debug.LogError("[Picker] panel == NULL (chưa kéo PickerPanel vào field panel).");
            return;
        }
        if (content == null)
        {
            Debug.LogError("[Picker] content == NULL (chưa kéo Content vào field content).");
            return;
        }
        if (itemPrefab == null)
        {
            Debug.LogError("[Picker] itemPrefab == NULL (chưa kéo prefab PickerItemButton).");
            return;
        }

        panel.SetActive(true);

        // ✅ đảm bảo nằm trên cùng
        panel.transform.SetAsLastSibling();

        Clear();

        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("[Picker] list rỗng.");
            return;
        }

        foreach (var it in list)
        {
            var row = Instantiate(itemPrefab, content);
            row.Set(getIcon(it), getName(it), () =>
            {
                onPick?.Invoke(it);
                Close();
            });
            spawned.Add(row);
        }

        Debug.Log($"[Picker] Open: spawned {spawned.Count} items.");
    }

    public void Close()
    {
        if (panel) panel.SetActive(false);
        Clear();
    }

    void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i].gameObject);
        spawned.Clear();
    }
}
