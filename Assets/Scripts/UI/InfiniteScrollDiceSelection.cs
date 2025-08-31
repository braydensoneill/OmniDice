using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollDiceSelection : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup horizontalLayoutGroup;

    public RectTransform[] itemList;

    [Header("Selection")]
    public UnityEngine.Events.UnityEvent<int> OnSelectedDiceChanged;

    [Header("Snap to Center")]
    public float snapDelay = 0.5f; // Delay before snapping starts
    public float snapSpeed = 2f; // Speed of snap animation

    private Coroutine snapCoroutine;
    private bool isSnapping = false;
    private float lastVelocityMagnitude;
    private bool wasBeingDragged = false;
    private float originalDecelerationRate;

    Vector2 OldVelocity;
    bool isUpdated;
    int itemsToAdd;
    int currentSelectedIndex = 0;

    // Cache frequently used calculations
    private float cachedItemWidth;
    private float cachedOneSetWidth;
    private float cachedStartPosition;

    // Prevent automatic selection changes during startup
    private bool allowAutoSelection = false;
    private bool hasUserScrolled = false;    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isUpdated = false;
        OldVelocity = Vector2.zero;

        // Store the original deceleration rate
        originalDecelerationRate = scrollRect.decelerationRate;

        // Cache these calculations once
        cachedItemWidth = itemList[0].rect.width + horizontalLayoutGroup.spacing;
        cachedOneSetWidth = itemList.Length * cachedItemWidth;
        cachedStartPosition = -cachedOneSetWidth; // Position of middle set

        // Simple approach: create 3 sets of items
        // [Set 1] [Set 2] [Set 3]
        // Start viewing Set 2, can scroll to Set 1 or Set 3, then loop

        // Create three complete sets
        for (int set = 0; set < 3; set++)
        {
            for (int i = 0; i < itemList.Length; i++)
            {
                RectTransform rectTransform = Instantiate(itemList[i], contentPanelTransform);
                rectTransform.SetAsLastSibling();
            }
        }

        // Position to start viewing the middle set (Set 2)

        // Find the index of 'classic' to center it initially
        int classicIndex = -1;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i].name.ToLower().Contains("classic"))
            {
                classicIndex = i;
                break;
            }
        }

        // Position content so the first dice (index 0) is perfectly centered
        // We need to account for the item's center position, not just its left edge
        float firstDiceCenter = cachedItemWidth * 0.5f; // Center of the first item
        float targetPosition = -cachedOneSetWidth - firstDiceCenter + (viewPortTransform.rect.width * 0.5f);
        contentPanelTransform.localPosition = new Vector3(targetPosition,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z);

        // Set the initial selected index to 0
        currentSelectedIndex = 0;

        // Set initial visual selection after a frame to ensure all UI is ready
        StartCoroutine(SetInitialVisualSelection());

        // Don't enable auto selection until user actually scrolls
    }

    private IEnumerator EnableAutoSelectionAfterDelay()
    {
        // Wait for user to actually scroll before enabling auto selection
        while (!hasUserScrolled)
        {
            yield return null;
        }
        allowAutoSelection = true;
        Debug.Log("InfiniteScroll: Auto selection enabled after user scroll");
    }
    private IEnumerator SetInitialVisualSelection()
    {
        yield return null; // Wait one frame
        UpdateVisualSelection();
    }    // Update is called once per frame
    void Update()
    {
        if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = OldVelocity;
        }

        // Detect if user has started scrolling
        if (!hasUserScrolled && (scrollRect.velocity.magnitude > 0.1f || Input.GetMouseButton(0) || Input.touchCount > 0))
        {
            hasUserScrolled = true;
            StartCoroutine(EnableAutoSelectionAfterDelay());
        }

        // Use cached values instead of recalculating every frame

        // Simple boundaries: if we scroll one complete set away from center, loop back

        // Right boundary - scrolled past Set 3 into empty space
        if (contentPanelTransform.localPosition.x > cachedStartPosition + cachedOneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(cachedOneSetWidth, 0, 0);
            isUpdated = true;
        }

        // Left boundary - scrolled past Set 1 into empty space
        if (contentPanelTransform.localPosition.x < cachedStartPosition - cachedOneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(cachedOneSetWidth, 0, 0);
            isUpdated = true;
        }

        // Update the currently selected dice based on scroll position (only if auto selection is enabled)
        if (allowAutoSelection)
        {
            UpdateSelectedDice();
        }

        // Handle snap to center functionality
        HandleSnapToCenter();
    }
    void UpdateSelectedDice()
    {
        // Use cached item width instead of recalculating

        // Calculate the center of the viewport in world space
        // Use the actual center of the viewport, not the left edge
        Vector3 viewportCenter = viewPortTransform.TransformPoint(new Vector3(viewPortTransform.rect.width * 0.5f, 0, 0));

        // Convert to local space of the content panel
        Vector3 localCenter = contentPanelTransform.InverseTransformPoint(viewportCenter);

        // Find which item is closest to the center
        // We need to account for the fact that items start at x = 0 in the content panel
        float firstItemCenter = cachedItemWidth * 0.5f; // Center of the first item
        float offsetFromFirstItem = localCenter.x - firstItemCenter;

        // Calculate which item index we're closest to
        float exactIndex = offsetFromFirstItem / cachedItemWidth;
        int closestIndex = Mathf.RoundToInt(exactIndex);

        // Add tolerance to prevent unwanted changes due to small positioning errors
        float distanceFromCurrentIndex = Mathf.Abs(exactIndex - currentSelectedIndex);
        float tolerance = 0.3f; // Only change if we're significantly closer to another item

        // Wrap the index to stay within the original itemList bounds
        closestIndex = ((closestIndex % itemList.Length) + itemList.Length) % itemList.Length;

        // Update selected dice if it changed AND we're significantly closer to the new item
        if (closestIndex != currentSelectedIndex && distanceFromCurrentIndex > tolerance)
        {
            Debug.Log($"InfiniteScroll: Selection change from {currentSelectedIndex} to {closestIndex} (distance: {distanceFromCurrentIndex:F2})");
            currentSelectedIndex = closestIndex;
            OnSelectedDiceChanged?.Invoke(currentSelectedIndex);
            UpdateVisualSelection();
        }
    }

    private void UpdateVisualSelection()
    {
        // Update all UI elements to show which one is selected
        for (int i = 0; i < contentPanelTransform.childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();

            // Add CanvasGroup if it doesn't exist
            if (canvasGroup == null)
            {
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }

            // Get the dice type name for this child
            string childDiceName = child.name.Replace("(Clone)", "").Trim();
            string selectedDiceName = GetSelectedDiceName();

            // Make selected dice fully opaque, others semi-transparent
            if (childDiceName.Equals(selectedDiceName, System.StringComparison.OrdinalIgnoreCase))
            {
                canvasGroup.alpha = 1.0f; // Fully opaque
            }
            else
            {
                canvasGroup.alpha = 0.5f; // Semi-transparent
            }
        }
    }

    private void HandleSnapToCenter()
    {
        // Don't interfere if already snapping
        if (isSnapping) return;

        bool currentlyBeingDragged = scrollRect.velocity.magnitude > 0.01f || Input.GetMouseButton(0) || (Input.touchCount > 0);

        // If user is actively dragging, cancel any pending snap
        if (currentlyBeingDragged)
        {
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
                // Restore original deceleration rate if it was modified
                if (isSnapping)
                {
                    scrollRect.decelerationRate = originalDecelerationRate;
                    isSnapping = false;
                }
            }
            wasBeingDragged = true;
            return;
        }

        // Check if we just stopped dragging (was dragging, now not dragging)
        if (wasBeingDragged && !currentlyBeingDragged)
        {
            // Start snap with delay
            if (snapCoroutine == null)
            {
                snapCoroutine = StartCoroutine(SnapToCenterAfterDelay());
            }
            wasBeingDragged = false;
        }
    }

    private IEnumerator SnapToCenterAfterDelay()
    {
        // No delay - snap immediately

        // Check if user started dragging again
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            snapCoroutine = null;
            yield break;
        }

        isSnapping = true;

        // Now disable deceleration and snap
        scrollRect.decelerationRate = 0f;
        scrollRect.velocity = Vector2.zero;

        // Find the UI element that's currently closest to the center of the viewport
        float halfViewportWidth = viewPortTransform.rect.width * 0.5f;
        Vector3 viewportCenterWorld = viewPortTransform.TransformPoint(new Vector3(halfViewportWidth, 0, 0));

        Transform closestItem = null;
        float minDistance = float.MaxValue;

        // Check all children in the content panel to find the one closest to center
        for (int i = 0; i < contentPanelTransform.childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            RectTransform childRect = child as RectTransform;

            // Calculate the center of this UI element
            Vector3 childCenterWorld = child.TransformPoint(childRect.rect.center);
            float distance = Mathf.Abs(childCenterWorld.x - viewportCenterWorld.x);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestItem = child;
            }
        }

        if (closestItem == null)
        {
            isSnapping = false;
            snapCoroutine = null;
            yield break;
        }

        // Calculate the offset needed to center this closest item
        RectTransform closestItemRect = closestItem as RectTransform;
        Vector3 closestItemCenterWorld = closestItem.TransformPoint(closestItemRect.rect.center);
        float offsetNeeded = closestItemCenterWorld.x - viewportCenterWorld.x;

        // Apply this offset to the current content position
        Vector3 targetPosition = new Vector3(contentPanelTransform.localPosition.x - offsetNeeded,
                                           contentPanelTransform.localPosition.y,
                                           contentPanelTransform.localPosition.z);

        Vector3 startPosition = contentPanelTransform.localPosition;
        float journey = 0f;

        while (journey <= 1f && isSnapping)
        {
            // Stop if user starts dragging again
            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                isSnapping = false;
                snapCoroutine = null;
                // Restore original deceleration rate
                scrollRect.decelerationRate = originalDecelerationRate;
                yield break;
            }

            journey += Time.deltaTime * snapSpeed;
            contentPanelTransform.localPosition = Vector3.Lerp(startPosition, targetPosition,
                                                              Mathf.SmoothStep(0f, 1f, journey));
            yield return null;
        }

        if (isSnapping)
        {
            contentPanelTransform.localPosition = targetPosition;
        }

        isSnapping = false;
        snapCoroutine = null;

        // Restore original deceleration rate
        scrollRect.decelerationRate = originalDecelerationRate;
    }

    // Public method to get the currently selected dice index
    public int GetSelectedDiceIndex()
    {
        return currentSelectedIndex;
    }

    // Public method to get the currently selected dice name (assuming itemList has names)
    public string GetSelectedDiceName()
    {
        if (itemList != null && itemList.Length > 0 && currentSelectedIndex >= 0 && currentSelectedIndex < itemList.Length)
        {
            return itemList[currentSelectedIndex].name.Replace("(Clone)", "").Trim();
        }
        return "";
    }
}