using UnityEngine;

public class InfiniteScrollSkinSelection : InfiniteScrollBase<string>
{
    [Header("Skin-Specific Settings")]
    public string defaultSkinName = "default";

    protected override string GetItemName(RectTransform item)
    {
        return item.name.Replace("(Clone)", "").Trim();
    }

    protected override string GetChildItemName(Transform child)
    {
        return child.name.Replace("(Clone)", "").Trim();
    }

    protected override void OnItemSelected(int index)
    {
        // Skin-specific selection logic
        Debug.Log($"Skin selected: {GetSelectedItemName()} (Index: {index})");

        // You can add skin-specific behavior here, such as:
        // - Updating skin manager
        // - Applying skin preview
        // - Saving skin preference
        // - Playing selection sound
    }

    protected override void SetInitialPosition()
    {
        // Find the index of default skin to center it initially
        int defaultIndex = -1;
        for (int i = 0; i < itemList.Length; i++)
        {
            if (itemList[i].name.ToLower().Contains(defaultSkinName.ToLower()))
            {
                defaultIndex = i;
                currentSelectedIndex = defaultIndex;
                break;
            }
        }

        base.SetInitialPosition();
    }

    // Skin-specific public methods
    public string GetSelectedSkinName()
    {
        return GetSelectedItemName();
    }

    public int GetSelectedSkinIndex()
    {
        return GetSelectedIndex();
    }
}