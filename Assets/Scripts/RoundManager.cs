using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemManager;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private GameObject green;
    [SerializeField] private GameObject orange;

    [SerializeField] public Vector3 greenStartPos;
    [SerializeField] public Vector3 orangeStartPos;
    public GameObject wonPlayer;

    public int orangeWonTime = 0;
    public int greenWonTime = 0;

    public delegate void RoundEnded(PlayerMovement wonPlayer, PlayerMovement lostPlayer);
    public event RoundEnded OnRoundEnded;
    public delegate void RoundStarted();
    public event RoundStarted OnRoundStarted;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;

    [Header("Sounds")]
    [SerializeField] private AudioClip roundEndSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Intro")]
    [SerializeField] private GameObject startText;
    [SerializeField] private AudioClip gameStartSound;

    private static RoundManager instance;

    public static RoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RoundManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("RoundManger");
                    instance = obj.AddComponent<RoundManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;

        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
   
    }
    private void Start()
    {
        OnItemSelected += OnSelectionEnded;
        MakeObstacle();
        OnRoundStarted?.Invoke();

        green.GetComponent<PlayerMovement>().stats.isAnchored = true;
        orange.GetComponent<PlayerMovement>().stats.isAnchored = true;

        bool confirmedStart = false;

        StartCoroutine(enumerator());
        IEnumerator enumerator()
        {
            while (true)
            {
                if (Input.anyKeyDown)
                {
                    confirmedStart = true;
                }
                if (confirmedStart)
                {
                    break;
                }
                yield return null;
            }

            startText.transform.localScale = Vector3.one;
            startText.transform.DOScale(.7f, .5f);

            AudioManager.Instance.PlayAudio(gameStartSound);

            yield return new WaitForSeconds(.5f);

            green.GetComponent<PlayerMovement>().stats.isAnchored = false;
            orange.GetComponent<PlayerMovement>().stats.isAnchored = false;

            

            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

            yield return null;
        }
    }

    public void PlayerDies(PlayerMovement diedPlayer)
    {
        
        green.GetComponent<PlayerMovement>().stats.isInvincible = true;
        orange.GetComponent<PlayerMovement>().stats.isInvincible = true;
        if (diedPlayer.plrType == PlayerTypes.Orange)
        {
            wonPlayer = green;
            greenWonTime++;
        }
        else if (diedPlayer.plrType == PlayerTypes.Green)
        {
            wonPlayer = orange;
            orangeWonTime++;
        }



        Time.timeScale = .1f;
        if (greenWonTime == 5 || orangeWonTime == 5)
        {
            foreach (Transform child in GameObject.Find("Obstacles").transform)
            {
                Destroy(child.gameObject);
            }
            if (greenWonTime == 5) { GameEnded(green.GetComponent<PlayerMovement>()); }
            else if (orangeWonTime == 5) { GameEnded(orange.GetComponent<PlayerMovement>()); }

            return; 
        }

        AudioManager.Instance.PlayAudio(roundEndSound);
        StartCoroutine(enumerator(wonPlayer.GetComponent<PlayerMovement>(), diedPlayer));
        OnRoundEnded?.Invoke(wonPlayer.GetComponent<PlayerMovement>(), diedPlayer);
    }

    private void OnSelectionEnded()
    {   
        green.GetComponent<PlayerMovement>().stats.isInvincible = false;
        orange.GetComponent<PlayerMovement>().stats.isInvincible = false;

        foreach (Transform child in GameObject.Find("Obstacles").transform)
        {
            Destroy(child.gameObject);
        }
        MakeObstacle();

        OnRoundStarted?.Invoke();
    }

    private void FightRoundEnd(PlayerMovement wonPlayer, PlayerMovement lostPlayer)
    {
        green.transform.position = greenStartPos;
        orange.transform.position= orangeStartPos;

        green.GetComponent<PlayerMovement>().stats.isAnchored = true;
        orange.GetComponent<PlayerMovement>().stats.isAnchored = true;

        Time.timeScale = 1f;
    }

    private void GameEnded(PlayerMovement wonPlayer)
    {
        Time.timeScale = 1f;
        orange.GetComponent<PlayerMovement>().stats.isAnchored = true;
        green.GetComponent<PlayerMovement>().stats.isAnchored = true;
        UIScript.Instance.GameEnded(wonPlayer);
    }

    IEnumerator enumerator(PlayerMovement wonPlayer, PlayerMovement lostPlayer)
    {
        yield return new WaitForSeconds(1f);
        FightRoundEnd(wonPlayer, lostPlayer);
    }

    private void MakeObstacle()
    {
        GameObject obj = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)]);
        obj.transform.SetParent(GameObject.Find("Obstacles").transform);

    }
}
