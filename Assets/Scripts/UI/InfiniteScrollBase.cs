using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InfiniteScrollBase<T> : MonoBehaviour
{
    [Header("Scroll Components")]
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup horizontalLayoutGroup;
    public RectTransform[] itemList;

    [Header("Selection")]
    public UnityEngine.Events.UnityEvent<int> OnSelectionChanged;

    [Header("Snap to Center")]
    public float snapDelay = 0.5f;
    public float snapSpeed = 2f;

    private Coroutine snapCoroutine;
    private bool isSnapping = false;
    private float originalDecelerationRate;
    private bool wasBeingDragged = false;

    protected Vector2 OldVelocity;
    protected bool isUpdated;
    protected int currentSelectedIndex = 0;

    // Cache frequently used calculations
    private float cachedItemWidth;
    private float cachedOneSetWidth;
    private float cachedStartPosition;

    // Prevent automatic selection changes during startup
    private bool allowAutoSelection = false;
    private bool hasUserScrolled = false;

    protected virtual void Start()
    {
        isUpdated = false;
        OldVelocity = Vector2.zero;
        originalDecelerationRate = scrollRect.decelerationRate;

        // Validate itemList before proceeding
        if (itemList == null || itemList.Length == 0)
        {
            Debug.LogError("itemList is empty! Make sure LoadSkinItems() or similar method populates itemList before calling base.Start()");
            return;
        }

        // Cache calculations
        cachedItemWidth = itemList[0].rect.width + horizontalLayoutGroup.spacing;
        cachedOneSetWidth = itemList.Length * cachedItemWidth;
        cachedStartPosition = -cachedOneSetWidth;

        CreateScrollSets();
        SetInitialPosition();
        StartCoroutine(SetInitialVisualSelection());
    }

    private void CreateScrollSets()
    {
        // Create three complete sets for infinite scrolling
        for (int set = 0; set < 3; set++)
        {
            for (int i = 0; i < itemList.Length; i++)
            {
                RectTransform rectTransform = Instantiate(itemList[i], contentPanelTransform);
                rectTransform.SetAsLastSibling();
            }
        }
    }

    protected virtual void SetInitialPosition()
    {
        // Position content so the first item is centered
        float firstItemCenter = cachedItemWidth * 0.5f;
        float targetPosition = -cachedOneSetWidth - firstItemCenter + (viewPortTransform.rect.width * 0.5f);
        contentPanelTransform.localPosition = new Vector3(targetPosition,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z);
        currentSelectedIndex = 0;
    }

    private IEnumerator SetInitialVisualSelection()
    {
        yield return null;
        UpdateVisualSelection();
    }

    protected virtual void Update()
    {
        if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = OldVelocity;
        }

        DetectUserScrolling();
        HandleInfiniteLoop();

        if (allowAutoSelection)
        {
            UpdateSelectedItem();
        }

        HandleSnapToCenter();
    }

    private void DetectUserScrolling()
    {
        if (!hasUserScrolled && (scrollRect.velocity.magnitude > 0.1f || Input.GetMouseButton(0) || Input.touchCount > 0))
        {
            hasUserScrolled = true;
            StartCoroutine(EnableAutoSelectionAfterDelay());
        }
    }

    private void HandleInfiniteLoop()
    {
        // Right boundary
        if (contentPanelTransform.localPosition.x > cachedStartPosition + cachedOneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(cachedOneSetWidth, 0, 0);
            isUpdated = true;
        }

        // Left boundary
        if (contentPanelTransform.localPosition.x < cachedStartPosition - cachedOneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(cachedOneSetWidth, 0, 0);
            isUpdated = true;
        }
    }

    private IEnumerator EnableAutoSelectionAfterDelay()
    {
        while (!hasUserScrolled)
        {
            yield return null;
        }
        allowAutoSelection = true;
    }

    private void UpdateSelectedItem()
    {
        Vector3 viewportCenter = viewPortTransform.TransformPoint(new Vector3(viewPortTransform.rect.width * 0.5f, 0, 0));
        Vector3 localCenter = contentPanelTransform.InverseTransformPoint(viewportCenter);

        float firstItemCenter = cachedItemWidth * 0.5f;
        float offsetFromFirstItem = localCenter.x - firstItemCenter;
        float exactIndex = offsetFromFirstItem / cachedItemWidth;
        int closestIndex = Mathf.RoundToInt(exactIndex);

        float distanceFromCurrentIndex = Mathf.Abs(exactIndex - currentSelectedIndex);
        float tolerance = 0.3f;

        closestIndex = ((closestIndex % itemList.Length) + itemList.Length) % itemList.Length;

        if (closestIndex != currentSelectedIndex && distanceFromCurrentIndex > tolerance)
        {
            currentSelectedIndex = closestIndex;
            OnSelectionChanged?.Invoke(currentSelectedIndex);
            OnItemSelected(currentSelectedIndex);
            UpdateVisualSelection();
        }
    }

    protected virtual void UpdateVisualSelection()
    {
        for (int i = 0; i < contentPanelTransform.childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            }

            string childName = GetChildItemName(child);
            string selectedName = GetSelectedItemName();

            canvasGroup.alpha = childName.Equals(selectedName, System.StringComparison.OrdinalIgnoreCase) ? 1.0f : 0.5f;
        }
    }

    private void HandleSnapToCenter()
    {
        if (isSnapping) return;

        bool currentlyBeingDragged = scrollRect.velocity.magnitude > 0.01f || Input.GetMouseButton(0) || (Input.touchCount > 0);

        if (currentlyBeingDragged)
        {
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
                if (isSnapping)
                {
                    scrollRect.decelerationRate = originalDecelerationRate;
                    isSnapping = false;
                }
            }
            wasBeingDragged = true;
            return;
        }

        if (wasBeingDragged && !currentlyBeingDragged)
        {
            if (snapCoroutine == null)
            {
                snapCoroutine = StartCoroutine(SnapToCenterAfterDelay());
            }
            wasBeingDragged = false;
        }
    }

    private IEnumerator SnapToCenterAfterDelay()
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            snapCoroutine = null;
            yield break;
        }

        isSnapping = true;
        scrollRect.decelerationRate = 0f;
        scrollRect.velocity = Vector2.zero;

        float halfViewportWidth = viewPortTransform.rect.width * 0.5f;
        Vector3 viewportCenterWorld = viewPortTransform.TransformPoint(new Vector3(halfViewportWidth, 0, 0));

        Transform closestItem = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < contentPanelTransform.childCount; i++)
        {
            Transform child = contentPanelTransform.GetChild(i);
            RectTransform childRect = child as RectTransform;
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

        RectTransform closestItemRect = closestItem as RectTransform;
        Vector3 closestItemCenterWorld = closestItem.TransformPoint(closestItemRect.rect.center);
        float offsetNeeded = closestItemCenterWorld.x - viewportCenterWorld.x;

        Vector3 targetPosition = new Vector3(contentPanelTransform.localPosition.x - offsetNeeded,
                                           contentPanelTransform.localPosition.y,
                                           contentPanelTransform.localPosition.z);

        Vector3 startPosition = contentPanelTransform.localPosition;
        float journey = 0f;

        while (journey <= 1f && isSnapping)
        {
            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                isSnapping = false;
                snapCoroutine = null;
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
        scrollRect.decelerationRate = originalDecelerationRate;
    }

    // Public accessors
    public int GetSelectedIndex() => currentSelectedIndex;
    public string GetSelectedItemName()
    {
        if (itemList != null && itemList.Length > 0 && currentSelectedIndex >= 0 && currentSelectedIndex < itemList.Length)
        {
            return GetItemName(itemList[currentSelectedIndex]);
        }
        return "";
    }

    // Abstract methods for child classes to implement
    protected abstract string GetItemName(RectTransform item);
    protected abstract string GetChildItemName(Transform child);
    protected abstract void OnItemSelected(int index);
}