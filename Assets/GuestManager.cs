using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
    public static GuestManager Instance;
    public GameObject guestFab;
    public float guestSpawnDelay;
    public Transform[] spawnPositions;

    private List<Guest> activeGuests = new List<Guest>();
    private float timeAtLastGuestSpawn;

    private float minDisgruntleTime = 20f, maxDisgruntleTime = 30f;

    [Header("AUDIO STUFF")] 
    public AudioClip[] correctItemSounds;
    public AudioClip[] wrongItemSounds;
    public AudioClip outOfTimeSound;
    private List<ItemData> CurrentlyRequestedItems
    {
        get;
        set;
    }

    public Guest GetHoveredGuest
    {
        get
        {
            for (int i = 0; i < activeGuests.Count; i++)
            {
                if (activeGuests[i].hovered) return activeGuests[i];
            }

            return null;
        }
    }

    private AudioSource aud;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        aud = GetComponent<AudioSource>();
    }
    void Start()
    {
        CurrentlyRequestedItems = new List<ItemData>();
        spawnPositions = new Transform[transform.childCount];
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            spawnPositions[i] = transform.GetChild(i);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Play)
        {
            if((GameManager.Instance.guestsServed + activeGuests.Count) < GameManager.Instance.requiredGuestsForRound && timeAtLastGuestSpawn + guestSpawnDelay < Time.time)
            {
                SpawnGuest();
                timeAtLastGuestSpawn = Time.time;

            }
        }
    }

    Guest SpawnGuest()
    {
        List<Transform> availSpawnPositions = spawnPositions.Where(x => x.childCount == 0).ToList();
        if(availSpawnPositions.Count == 0)
        {
            Debug.Log("Nowhere to spawn!");
            return null;
        }

        if (InventoryManager.Instance.currentItems.Count-activeGuests.Count <= 0)
        {
            Debug.Log("No unrequested items available");
            return null;
        }
        Guest newGuest = Instantiate(guestFab).GetComponent<Guest>();
        newGuest.rect = newGuest.GetComponent<RectTransform>();
        newGuest.RequestedItem = InventoryManager.Instance.GetUniqueAvailableItem(CurrentlyRequestedItems);
        activeGuests.Add(newGuest);
        newGuest.transform.SetParent(availSpawnPositions[0]);
        newGuest.transform.localScale = Vector3.one;
        newGuest.rect.offsetMin = Vector2.zero;//.sizeDelta = Vector2.zero;
        newGuest.rect.offsetMax = Vector2.zero;//.sizeDelta = Vector2.zero;
        newGuest.transform.localPosition = Vector3.zero;
        newGuest.Configure(Random.Range(minDisgruntleTime,maxDisgruntleTime));
        CurrentlyRequestedItems = GetCurrentlyRequestedItems();
        return newGuest;
    }

    List<ItemData> GetCurrentlyRequestedItems()
    {
        List<ItemData> newRequestedItems = new List<ItemData>();
        foreach (var guest in activeGuests)
        {
            newRequestedItems.Add(guest.RequestedItem);
        }

        return newRequestedItems;
    }

    public void DestroyGuest(Guest guest, bool failure)
    {
        guest.isBeingDestroyed = true;
        activeGuests.Remove(guest);
        if (!failure)
        {
            guest.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                Destroy(guest.gameObject)
                );
        }
        else
        {
            Color redNoAlpha = Color.red;
            redNoAlpha.a = 0f;
            guest.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                Destroy(guest.gameObject)
                );
            guest.faceImage.material.DOColor(redNoAlpha,1f);
            guest.faceImage.transform.DOScale(Vector3.one * 1.5f, 1f);
            guest.textPanel.SetActive(false);
        }
    }

    public void GuestTimeout(Guest guest)
    {
        Debug.Log("A GUEST TIMED OUT!");
        GameManager.Instance.ItemGivenToGuest(false);
        aud.PlayOneShot(outOfTimeSound);
        DestroyGuest(guest, true);
    }
    public void CheckItemAgainstGuest(ItemData data)
    {
        Debug.Log("Hovered guest : " + GetHoveredGuest);
        Guest hoveredGuest = GetHoveredGuest;
        if (hoveredGuest != null)
        {
            if (InventoryManager.Instance.CurrentlySelectedItem != null)
            {
                if (InventoryManager.Instance.CurrentlySelectedItem.data.type == hoveredGuest.RequestedItem.type &&
                    InventoryManager.Instance.CurrentlySelectedItem.data.paletteInfo.name == hoveredGuest.RequestedItem.paletteInfo.name)
                {
                    GameManager.Instance.ItemGivenToGuest(true);
                    aud.PlayOneShot(correctItemSounds[Random.Range(0,correctItemSounds.Length)]);
                    InventoryManager.Instance.RemoveItem(InventoryManager.Instance.CurrentlySelectedItem);
                    DestroyGuest(hoveredGuest, false);
                }
                else
                { 
                    GameManager.Instance.ItemGivenToGuest(false);
                    aud.PlayOneShot(wrongItemSounds[Random.Range(0,wrongItemSounds.Length)]);
                    InventoryManager.Instance.CurrentlySelectedItem.ReturnToOrigPos();
                    DestroyGuest(hoveredGuest, true);
                }
            }
        }
    }

    public void ClearAllGuests()
    {
        foreach (var guest in activeGuests)
        {
            guest.transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                Destroy(guest.gameObject)
                );
        }

        activeGuests.Clear();
    }

}
