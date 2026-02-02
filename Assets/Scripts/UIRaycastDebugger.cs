using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var es = EventSystem.current;
        if (es == null) { Debug.Log("No EventSystem"); return; }

        var ped = new PointerEventData(es) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        es.RaycastAll(ped, results);

        if (results.Count == 0)
        {
            Debug.Log("UI Raycast: nothing hit");
            return;
        }

        Debug.Log("UI Raycast hits (top first):");
        for (int i = 0; i < Mathf.Min(results.Count, 10); i++)
        {
            Debug.Log($"{i}: {results[i].gameObject.name} (module: {results[i].module})");
        }
    }
}
