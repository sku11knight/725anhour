using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryManager : MonoBehaviour
{

    [SerializeField] private int maxRotDelta;
    public PaletteInfo[] allPalettes;
    private int numberOfExistingItems = 20, poolSize = 300;
    public Sprite[] itemSprites;

    private RectTransform rect;
    [Header("STATIC POOL STUFF")]
    public GameObject lostAndFoundFab;
    public Queue<LostAndFoundItem> itemPool;
    public List<LostAndFoundItem> currentItems;

    public static InventoryManager Instance;

    public LostAndFoundItem CurrentlySelectedItem
    {
        get;
        set;
    }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }else Destroy(gameObject);
        rect = GetComponent<RectTransform>();
        itemPool = new Queue<LostAndFoundItem>();
        for (int i = 0; i < poolSize; i++)
        {
            LostAndFoundItem newItem = Instantiate(lostAndFoundFab).GetComponent<LostAndFoundItem>();
            newItem.lostAndFoundParent = rect;
            newItem.transform.SetParent(gameObject.transform);
            newItem.gameObject.SetActive(false);
            itemPool.Enqueue(newItem);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void SpawnItems(ItemType[] allowedItems, int requiredGuests, int numberOfItemsMod,float maxItemSize)
    {

        foreach (LostAndFoundItem item in currentItems)
        {
            itemPool.Enqueue(item);
        }

        currentItems.Clear();
        numberOfExistingItems = requiredGuests * numberOfItemsMod; 
        for(int i =0;i<numberOfExistingItems;i++)
        {
            LostAndFoundItem item = GrabItem();
            ItemData newData = new ItemData();
            newData.paletteInfo = allPalettes[Random.Range(0, allPalettes.Length)];
            newData.type = allowedItems[Random.Range(0, allowedItems.Length)];//(ItemType) Random.Range(0,Enum.GetNames(typeof(ItemType)).Length);
            newData.itemSprite = itemSprites[(int)newData.type];
            item.transform.localPosition = new Vector2(
                Random.Range(rect.rect.xMin, rect.rect.xMax),
                Random.Range(rect.rect.yMin, rect.rect.yMax)
            );
            item.transform.localScale = Vector3.zero;//Vector3.one * Random.Range(0.8f, 1.2f);
            item.transform.DOScale(Vector3.one * Random.Range(0.5f, maxItemSize), 0.25f).SetDelay(0.01f*i);
            item.transform.localEulerAngles = Vector3.forward * Random.Range(-maxRotDelta, maxRotDelta);
            item.ApplyItemData(newData);
            item.gameObject.SetActive(true);
        }
        
    }


    public LostAndFoundItem GrabItem()
    {
        LostAndFoundItem item = itemPool.Dequeue();
        currentItems.Add(item);
        return item;
    }
    public void RemoveItem(LostAndFoundItem item)
    {
        item.gameObject.SetActive(false);
        itemPool.Enqueue(item);
        currentItems.Remove(item);
    }

    public ItemData GetUniqueAvailableItem(List<ItemData> currentlyRequestedItems)
    {
        List<LostAndFoundItem> uniqueItems = currentItems.Where(x => !currentlyRequestedItems.Contains(x.data)).ToList();
        return uniqueItems[Random.Range(0, uniqueItems.Count-1)].data;
    }

    public void ClearAllItems()
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            currentItems[i].transform.DOScale(Vector3.zero, 0.5f).SetDelay(i * 0.01f).OnComplete(()=>RemoveItem(currentItems[i]));
        }
    }
}
