using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Guest : MonoBehaviour
{

    [HideInInspector]
    public ItemData RequestedItem
    {
        get { return requestedItem; }
        set
        {
            image.sprite = value.itemSprite;
            image.material.SetTexture("_PaletteTex",value.paletteInfo.palette);
            requestedItem = value;
        }
    }

    private ItemData requestedItem;
    private string demandMessage = "<shake a=0.25>WHERE</shake> \n ARE MY";

    public float timeRemaining, maxTimeRemaining;
    
    public TextMeshProUGUI textObj;
    public Image image, faceImage;

    public bool isBeingDestroyed;

    [HideInInspector]
    public RectTransform rect;

    public bool hovered
    {
        get
        {
            Vector2 localMousePosition = rect.InverseTransformPoint(Input.mousePosition);
            return rect.rect.Contains(localMousePosition);
        }
    }

    public bool isHovered;
    // Start is called before the first frame update

    void Awake()
    {
        faceImage = GetComponent<Image>();
    }
    void Start()
    {
        image.material = Instantiate<Material>(image.material);
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBeingDestroyed)
        {
            
            isHovered = hovered;
            timeRemaining -= Time.deltaTime;
            float normalizedRemainingTime = Mathf.Abs((timeRemaining / maxTimeRemaining) - 1);
            faceImage.color = Color.Lerp(Color.white,Color.red, normalizedRemainingTime);
            if (!DOTween.IsTweening(gameObject))
            {
                gameObject.transform.DOShakePosition(0.5f, 1f, (int)Mathf.Lerp(1,75,normalizedRemainingTime), 10f,false,false);

            }
            if (timeRemaining <= 0f)
            {
                GuestManager.Instance.GuestTimeout(this);
            }
        }
    }

    private void FixedUpdate()
    {
    }

    public void CheckIfCorrectItem()
    {
        if (hovered)
        {
        }
    }

    public void Configure(float disgruntledTime)
    {
        textObj.text = demandMessage;
        image.transform.localScale = Vector3.zero;
        image.transform.DOScale(Vector3.one, 1f);
        maxTimeRemaining = disgruntledTime;
        timeRemaining = disgruntledTime;
    }
}
