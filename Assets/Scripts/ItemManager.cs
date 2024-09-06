using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] GameObject defaultSwordStats;


    [SerializeField] public GameObject playerOrange;
    [SerializeField] public GameObject playerGreen;
    [SerializeField] private PlayerMovement pickingPlayer;

    public delegate void ItemSelected();
    public static event ItemSelected OnItemSelected;

    private static ItemManager instance;

    public GameObject skillCard;
    public GameObject statCard;
    public GameObject passiveCard;

    [Header("Sounds")]
    [SerializeField] private AudioClip cardPickedSound;
    [SerializeField] private AudioClip cardPickingStartedSound;

    public static ItemManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("ItemManager");
                    instance = obj.AddComponent<ItemManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        // �̱����� ���� �ڵ�
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
        GiveDefaultSword();
    }

    private void GiveDefaultSword()
    {
        playerGreen.GetComponent<PlayerMovement>().swordGameObject.GetComponent<SwordScript>().ApplySwordChanges(defaultSwordStats.GetComponent<SwordSettings>());
        playerOrange.GetComponent<PlayerMovement>().swordGameObject.GetComponent<SwordScript>().ApplySwordChanges(defaultSwordStats.GetComponent<SwordSettings>());
    }

    public void StartPicking(PlayerMovement lostPlr, GameObject ui, GameObject[] skillCards, GameObject[] statCards, GameObject[] passiveCards)
    {
        pickingPlayer = lostPlr;

        skillCard = skillCards[UnityEngine.Random.Range(0, skillCards.Length)];
        statCard = statCards[UnityEngine.Random.Range(0, statCards.Length)];
        passiveCard = passiveCards[UnityEngine.Random.Range(0, passiveCards.Length)];
        
        skillCard = Instantiate(skillCard);
        statCard = Instantiate(statCard);
        passiveCard = Instantiate(passiveCard);

        skillCard.GetComponent<SkillCards>().type = CardTypes.Skill;
        statCard.GetComponent<SkillCards>().type = CardTypes.Stat;
        passiveCard.GetComponent<SkillCards>().type = CardTypes.Passive;

        skillCard.transform.position = ui.transform.Find("First").transform.position + new Vector3(0, -5, 0);
        statCard.transform.position = ui.transform.Find("Middle").transform.position + new Vector3(0, -5, 0);
        passiveCard.transform.position = ui.transform.Find("Last").transform.position + new Vector3(0, -5, 0);

        skillCard.transform.DOMoveY(0, 1.2f);
        statCard.transform.DOMoveY(0, 1.2f);
        passiveCard.transform.DOMoveY(0, 1.2f);

        AudioManager.Instance.PlayAudio(cardPickingStartedSound);

        StartCoroutine(enumerator());

        ui.transform.Find("SelectionBackGround").transform.position = new Vector3(0, -10, 0);
        ui.transform.Find("SelectionBackGround").transform.DOMoveY(0, .65f);

        ui.transform.Find("UIS").transform.position = new Vector3(0, -10, 0);
        ui.transform.Find("UIS").transform.DOMoveY(0, .65f);

        switch (lostPlr.plrType)
        {
            case PlayerTypes.Orange:
                ui.transform.Find("UIS").Find("Info2").GetComponent<TextMeshProUGUI>().text = "<1, 2, 3 Ű�� Ȧ���� ���� ����.>";
                ui.transform.Find("UIS").Find("SelectingPlayer").GetComponent<TextMeshProUGUI>().text = "��Ȳ�� �÷��̾ ���׷��̵带 �����մϴ�.";
                break;
            case PlayerTypes.Green:
                ui.transform.Find("UIS").Find("Info2").GetComponent<TextMeshProUGUI>().text = "<L, ;, ' Ű�� Ȧ���� ���� ����.>";
                ui.transform.Find("UIS").Find("SelectingPlayer").GetComponent<TextMeshProUGUI>().text = "�ʷϻ� �÷��̾ ���׷��̵带 �����մϴ�.";
                break;
        }
        ui.transform.Find("UIS").Find("Info2").GetComponent<TextMeshProUGUI>().color = lostPlr.color;
        ui.transform.Find("UIS").Find("SelectingPlayer").GetComponent<TextMeshProUGUI>().color = lostPlr.color;

        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(3f);
            GameObject[] cards = new GameObject[3] { skillCard, statCard, passiveCard };
            foreach (GameObject card in cards)
            {
                card.name = card.name.Replace("(Clone)", "");
                card.GetComponent<SkillCards>().originalPos = card.transform.position;
                card.GetComponent<SkillCards>().canSelect = true;

            }
            yield return null;
        }
    }

    public void OnCardClick(SkillCards selectedCard)
    {
        if (selectedCard.type == CardTypes.Skill)
        {
            pickingPlayer.GiveSkill(selectedCard);
        }
        else if (selectedCard.type == CardTypes.Passive)
        {
            pickingPlayer.swordGameObject.GetComponent<SwordScript>().ApplySwordChanges(selectedCard.skillManager.GetComponent<SwordSettings>());
        }
        else { pickingPlayer.GiveStats(selectedCard); }


        GameObject ui = GameObject.Find("SelectionUI");

        ui.transform.Find("SelectionBackGround").transform.DOMoveY(-10, .35f);

        ui.transform.Find("UIS").transform.DOMoveY(-10, .35f);

        GameObject[] cards = new GameObject[3] { skillCard, statCard, passiveCard };
        foreach (GameObject card in cards)
        {
            card.transform.DOMoveY(-10, .5f);
            card.GetComponent<SkillCards>().canSelect = false;
        }

        AudioManager.Instance.PlayAudio(cardPickedSound);
        pickingPlayer = null;


        StartCoroutine(enumerator());
        OnItemSelected?.Invoke();
        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(.2f);

            Destroy(skillCard);
            Destroy(statCard);
            Destroy(passiveCard);

            skillCard = null;
            statCard = null;
            passiveCard = null;

            yield return null;
        }
    }

    public void ApplyUpgrade(SkillCards pickedCard)
    {

    }
}
