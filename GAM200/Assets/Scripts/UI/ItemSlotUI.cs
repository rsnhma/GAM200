using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemID;
    public System.Action onUse;

    private Transform originalParent;
    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(parentCanvas.transform); // bring to top of canvas
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // follow mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Convert mouse position to world
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Raycast to check if dropped on a world object
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            TVInteraction tv = hit.collider.GetComponent<TVInteraction>();
            if (tv != null && itemID == "vhs_tape")
            {
                // Call the VHS use logic
                onUse?.Invoke();

                // Remove from UI
                Destroy(gameObject);
                return;
            }
        }

        // Snap back if not dropped on TV
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}
