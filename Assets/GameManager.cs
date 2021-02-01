using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum GameState
{
    Idle,
    Play,
    End
}
[System.Serializable]
public class RoundData
{
    public int TotalGuestsToServe, numberOfItemsMod = 3;
    public float maxItemSize;
    public ItemType[] allowedItems;
}
public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public int guestsServed;
    public int requiredGuestsForRound;

    public int currentLives;

    public TextMeshProUGUI scoreTxt, gameOverText;
    public Image backgroundImage;
    public GameObject gameOverScreen, idleScreen;
    public Button gameOverButton;
    [SerializeField] private Transform nowWithParent;
    
    public Image[] allLives;

    public RoundData[] allRounds;

    private int roundIndex = 0;
    private RoundData currentRound;
    private GameState currentState;
    private AudioSource aud;
    

    private bool lost;

    public GameState CurrentState
    {
        get { return currentState; }
    }
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        for (int i = 0; i < nowWithParent.childCount; i++)
        {
            nowWithParent.GetChild(i).name = Enum.GetName(typeof(ItemType), i).ToString();
            nowWithParent.GetChild(i).gameObject.SetActive(false);
        }

        aud = GetComponent<AudioSource>();
    }
    void Start()
    {
        SwitchState(GameState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        idleScreen.SetActive(currentState == GameState.Idle);
        gameOverScreen.SetActive(currentState == GameState.End);
        switch (currentState)
        {
           case GameState.Play:
                scoreTxt.text = "<u><i>SATISFIED</i>\nCUSTOMERS</u>\n<size=300%><u>" + guestsServed.ToString() + "</u>\n" + requiredGuestsForRound;
                if (currentLives == 0)
                {
                    lost = true;
                    SwitchState(GameState.End);
                }
                for (int i = 0; i < allLives.Length; i++)
                {
                    if (i > currentLives-1) allLives[i].gameObject.SetActive(false);
                }
                if(guestsServed >= requiredGuestsForRound) SwitchState(GameState.End);
               break;
        }
    }

    public void SwitchState(GameState state)
    {
        currentState = state;
        switch (currentState)
        {
            case GameState.Idle:
                break;
            case GameState.Play:
                lost = false;
                aud.Play();
                currentRound = allRounds[roundIndex];
                backgroundImage.material.SetTexture("_PaletteTex",InventoryManager.Instance.allPalettes[roundIndex].palette);
                StartRound(currentRound);
                break;
            case GameState.End:
                aud.Stop();
                InventoryManager.Instance.ClearAllItems();
                GuestManager.Instance.ClearAllGuests();
                gameOverButton.onClick.RemoveAllListeners();
                if (lost)
                {
                    gameOverText.text = "Round <color=red><b>FAILED</b></color>";
                    gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = "Retry?";
                    gameOverButton.onClick.AddListener(()=>
                        SwitchState(GameState.Play));
                }
                else
                {
                    roundIndex++;
                    if (roundIndex >= allRounds.Length)
                    {
                        gameOverText.text = "<b><color=green><size=200%>YOU FREAKING WON!";
                        gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = "here's my main squeeze!";
                        gameOverButton.onClick.AddListener(()=>
                            ALessBadGame()
                            );
                    }
                    else
                    {
                        gameOverText.text = "Round <color=green><b>VICTORY</b></color>\nNOW WITH:";
                        gameOverButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next Level";
                        for (int i = 0; i < allRounds[roundIndex].allowedItems.Length; i++)
                        {
                            nowWithParent.transform.Find(Enum.GetName(typeof(ItemType),allRounds[roundIndex].allowedItems[i])).gameObject.SetActive(true);
                        }
                        gameOverButton.onClick.AddListener(()=>
                            SwitchState(GameState.Play));
                    }
                }
                break;
        }
    }

    public void StartGame()
    {
        SwitchState(GameState.Play);
    }

    public void ALessBadGame()
    {
        Application.OpenURL("http://escapeacademygame.com");
    }

    void StartRound(RoundData round)
    {
        guestsServed = 0;
        requiredGuestsForRound = round.TotalGuestsToServe;
        currentLives = allLives.Length;
        InventoryManager.Instance.SpawnItems(round.allowedItems, requiredGuestsForRound, round.numberOfItemsMod, round.maxItemSize);
        for (int i = 0; i < allLives.Length; i++)
        {
            allLives[i].gameObject.SetActive(true);
        }
    }

    public void ItemGivenToGuest(bool correctItem)
    {
        if (correctItem)
        {
            guestsServed++;
        }
        else
        {
            currentLives--;
        }
        
    }
}
