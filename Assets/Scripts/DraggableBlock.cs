using UnityEngine;
using UnityEngine.EventSystems;

// Requires a CanvasGroup component to manage raycasts while dragging
[RequireComponent(typeof(CanvasGroup))]
public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int slotIndex; // Assigned automatically by BlockSlotsManager
    [HideInInspector] public BlockSlotsManager slotManager;

    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Vector3 dragOffset;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
        originalParent = transform.parent;

        // Move the block to the highest layer so it renders over everything while dragging
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            transform.SetParent(parentCanvas.transform);
        }
        else
        {
            transform.SetParent(null);
        }
        transform.SetAsLastSibling();

        // Turn off raycasts so the block doesn't block pointer events
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        // Calculate offset so the block doesn't awkwardly jump to the cursor center
        dragOffset = transform.position - GetPointerWorldPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Make the block follow the cursor smoothly
        transform.position = GetPointerWorldPosition(eventData) + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Turn raycasts back on
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // Find the grid manager to check if we dropped it in a valid location
        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
#pragma warning disable CS0618
            gridManager = FindObjectOfType<GridManager>();
#pragma warning restore CS0618
        }

        if (gridManager != null && gridManager.TryPlaceBlock(this))
        {
            // --- SUCCESSFUL PLACEMENT ---
            // Tell the Slot Manager to generate a new block for this slot
            if (slotManager != null)
            {
                slotManager.OnBlockSuccessfullyPlaced(slotIndex);
            }

            // Destroy this draggable block container (GridManager has already reparented the visual tiles to the board)
            Destroy(gameObject);
        }
        else
        {
            // --- INVALID PLACEMENT --- (e.g. dropped off-grid or overlapping existing blocks)
            // Snap back to the original slot
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }

    /// <summary>
    /// Converts pointer/mouse screen position into appropriate world coordinates.
    /// </summary>
    private Vector3 GetPointerWorldPosition(PointerEventData eventData)
    {
        // 1. Try event camera (Screen Space Camera or World Space UI)
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;

        if (cam != null)
        {
            float distanceToCamera = Mathf.Abs(transform.position.z - cam.transform.position.z);
            Vector3 screenPos = new Vector3(eventData.position.x, eventData.position.y, distanceToCamera);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = transform.position.z;
            return worldPos;
        }

        // 2. Fallback to raw mouse position
        return Input.mousePosition;
    }
}