using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup horizontalLayoutGroup;

    public RectTransform[] itemList;

    Vector2 OldVelocity;
    bool isUpdated;
    int itemsToAdd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isUpdated = false;
        OldVelocity = Vector2.zero;

        float itemWidth = itemList[0].rect.width + horizontalLayoutGroup.spacing;

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
        float oneSetWidth = itemList.Length * itemWidth;
        contentPanelTransform.localPosition = new Vector3(-oneSetWidth,
            contentPanelTransform.localPosition.y,
            contentPanelTransform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = OldVelocity;
        }

        float itemWidth = itemList[0].rect.width + horizontalLayoutGroup.spacing;
        float oneSetWidth = itemList.Length * itemWidth;
        float startPosition = -oneSetWidth; // Position of middle set

        // Simple boundaries: if we scroll one complete set away from center, loop back

        // Right boundary - scrolled past Set 3 into empty space
        if (contentPanelTransform.localPosition.x > startPosition + oneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(oneSetWidth, 0, 0);
            isUpdated = true;
        }

        // Left boundary - scrolled past Set 1 into empty space
        if (contentPanelTransform.localPosition.x < startPosition - oneSetWidth * 0.5f)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(oneSetWidth, 0, 0);
            isUpdated = true;
        }
    }
}