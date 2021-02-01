using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public enum ItemType
{
    Pants,
    Suitcases,
    Shirt,
    Socks,
    Umbrellas,
    Beanies
}

[System.Serializable]
public class PaletteInfo
{
    public Texture2D palette;
    public string name;
}

[System.Serializable]
public class ItemData
{
    public ItemType type;
    public PaletteInfo paletteInfo;
    public Sprite itemSprite;

}
public class LostAndFoundItem : MonoBehaviour
{
    private Image image;

    public ItemData data;

    public bool itemSelected;

    public Vector2 origPos;

    public RectTransform lostAndFoundParent;

    public bool InsideLostAndFound
    {
        get
        {
            Vector2 localPos = lostAndFoundParent.InverseTransformPoint(transform.position);
            return lostAndFoundParent.rect.Contains(localPos);
        }
    }
    void Awake()
    {
        image = GetComponent<Image>();
        image.material = Instantiate<Material>(image.material);
        image.alphaHitTestMinimumThreshold = 1f;
    }

    public void ApplyItemData(ItemData newItemData)
    {
        data = newItemData;
        image.material.SetTexture("_PaletteTex", data.paletteInfo.palette);
        image.sprite = data.itemSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (itemSelected)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void Highlight(bool shouldHighlight)
    {
        
    }

    public void SelectItem(bool shouldSelect)
    {
        if (itemSelected && !shouldSelect)
        {
            GuestManager.Instance.CheckItemAgainstGuest(data);
        }
        itemSelected = shouldSelect;
        if (itemSelected)
        {
            origPos = transform.position;
            InventoryManager.Instance.CurrentlySelectedItem = this;
        }
        else if(!InsideLostAndFound)
        {
            ReturnToOrigPos();
        }
    }

    public void ReturnToOrigPos()
    {
        transform.DOMove(origPos, 1f);
    }
}
