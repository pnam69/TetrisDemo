using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Runtime helper: click/tap on screen to list UI elements that received the pointer raycast.
// Attach to any GameObject in the scene (e.g. the root Canvas) and open the Console.
public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Input.mousePosition;
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = pos };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            Debug.Log($"UI Raycast at {pos} -> {results.Count} results");

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                GameObject go = r.gameObject;
                var graphic = go.GetComponent<Graphic>();
                var cg = go.GetComponentInParent<CanvasGroup>();
                var canvas = go.GetComponentInParent<Canvas>();

                bool raycastTarget = graphic != null ? graphic.raycastTarget : false;
                bool blocks = cg != null ? cg.blocksRaycasts : false;
                int canvasOrder = canvas != null ? canvas.sortingOrder : 0;

                Debug.LogFormat("{0}. Name='{1}'  module={2}  depth={3}  index={4}  sortOrder={5}  raycastTarget={6}  canvasGroup.blocksRaycasts={7}",
                    i, go.name, r.module != null ? r.module.name : "(no module)", r.depth, r.index, canvasOrder, raycastTarget, blocks);
            }

            if (results.Count == 0)
                Debug.Log("No UI receives the raycast. Check EventSystem, GraphicRaycaster and Canvas ordering.");
        }
    }
}
