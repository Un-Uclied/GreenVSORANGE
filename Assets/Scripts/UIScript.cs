using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class UIScript : MonoBehaviour
{
    public Volume volume;

    [SerializeField] Transform cameraHolder;

    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject orange;
    [SerializeField] private GameObject green;

    [SerializeField] private PlayerMovement plrOrange;
    [SerializeField] private PlayerMovement plrGreen;

    [SerializeField] private GameObject hitText;
    [SerializeField] private GameObject roundEndedText;
    [SerializeField] private Color orangeHittedColor;
    [SerializeField] private Color greenHittedColor;


    [SerializeField] private Color BGHitGreenColor;
    [SerializeField] private Color BGHitOrangeColor;
    [SerializeField] private Color BGColor;
    [SerializeField] private GameObject backGround;

    [Header("Score")]
    [SerializeField] private GameObject scoreCircle;
    [SerializeField] private GameObject scoreBoard;

    [Header("Selection")]
    [SerializeField] private GameObject SelectionUI;
    [SerializeField] private GameObject[] skillCards;
    [SerializeField] private GameObject[] statCards;
    [SerializeField] private GameObject[] passiveCards;

    [Header("GameEnd")]
    [SerializeField] private GameObject GreenWonSprite;
    [SerializeField] private GameObject OrangeWonSprite;
    
    [Header("Win")]
    [SerializeField] private AudioClip reverseSnare;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip[] tauntSounds;

    private Coroutine corou;
    private Coroutine backgroundCorou;
    private Coroutine boomCorouGreen;
    private Coroutine boomCorouOrange;

    private static UIScript instance;

    public static UIScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIScript>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("UIHandler");
                    instance = obj.AddComponent<UIScript>();
                }
            }

            return instance;
        }
    }

    private void Update()
    {
        green.transform.Find("Confused").gameObject.SetActive(plrGreen.stats.isConfused);
        green.transform.Find("Iced").gameObject.SetActive(plrGreen.stats.isIceBerged);
        green.transform.Find("Stuned").gameObject.SetActive(plrGreen.stats.isAnchored);

        orange.transform.Find("Confused").gameObject.SetActive(plrOrange.stats.isConfused);
        orange.transform.Find("Iced").gameObject.SetActive(plrOrange.stats.isIceBerged);
        orange.transform.Find("Stuned").gameObject.SetActive(plrOrange.stats.isAnchored);
    }

    private void Awake()
    {
        // ½Ì±ÛÅæÀ» À§ÇÑ ÄÚµå
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        RoundManager.Instance.OnRoundEnded += RoundEnded;
        RoundManager.Instance.OnRoundStarted += RoundStarted;
    }

    public void OnHit(PlayerMovement hittedPlr)
    {
        if (hittedPlr.stats.health <= 0) { return; }

        switch (hittedPlr.plrType)
        {
            case PlayerTypes.Orange:
                hitText.GetComponent<TextMeshProUGUI>().color = orangeHittedColor;
                break;
            case PlayerTypes.Green:
                hitText.GetComponent<TextMeshProUGUI>().color = greenHittedColor;
                break;
        }

        if (corou != null) { StopCoroutine(corou); }
        hitText.GetComponentInParent<TextMeshProUGUI>().fontSize = 0;
        corou = StartCoroutine(SizeBoomText(hitText.GetComponent<TextMeshProUGUI>(), 120f, .35f, .25f));
        hitText.GetComponent<RectTransform>().DOComplete();
        hitText.GetComponent<RectTransform>().DOShakeRotation(.5f, 10f, 100);

        ChangeBGColor(hittedPlr);

        cameraHolder.DOComplete();
        cameraHolder.DOShakePosition(.5f, .25f, 100);
    }

    private void ChangeBGColor(PlayerMovement hittedPlr)
    {
        Color color;
        switch (hittedPlr.plrType)
        {
            case PlayerTypes.Orange:
                color = BGHitOrangeColor;
                break;

            case PlayerTypes.Green:
                color = BGHitGreenColor;
                break;

            default:
                color = Color.white;
                break;
        }

        SpriteRenderer bg = backGround.GetComponent<SpriteRenderer>();

        if (backgroundCorou != null) { StopCoroutine(backgroundCorou); bg.color = BGColor; }
        bg.color = color;
        backgroundCorou = StartCoroutine(ColorLerp(bg, bg.color, BGColor, .5f));
    }

    public void UpdateHealth(PlayerMovement hittedPlr)
    {
        switch (hittedPlr.plrType)
        {
            case PlayerTypes.Green:
                TextMeshProUGUI healthTextGreen = green.transform.Find("Health").GetComponent<TextMeshProUGUI>();
                RectTransform healthBarFillGreen = green.transform.Find("Fill").GetComponent<RectTransform>();
                if (plrGreen.stats.health >= 0) { healthTextGreen.text = plrGreen.stats.health.ToString(); }
                else { healthTextGreen.text = 0.ToString(); }
                healthBarFillGreen.DOSizeDelta(new Vector2(plrGreen.stats.health * 4, healthBarFillGreen.sizeDelta.y), .1f);
                if (boomCorouGreen != null) { StopCoroutine(boomCorouGreen); healthTextGreen.fontSize = 60; }
                boomCorouGreen = StartCoroutine(SizeBoomText(healthTextGreen, 25f, .008f, .1f));
                 
                break;

            case PlayerTypes.Orange:
                TextMeshProUGUI healthTextOrange = orange.transform.Find("Health").GetComponent<TextMeshProUGUI>();
                RectTransform healthBarFillOrange = orange.transform.Find("Fill").GetComponent<RectTransform>();
                if (plrOrange.stats.health >= 0) { healthTextOrange.text = plrOrange.stats.health.ToString(); }
                else { healthTextOrange.text = 0.ToString(); }
                healthBarFillOrange.DOSizeDelta(new Vector2(plrOrange.stats.health * 4, healthBarFillOrange.sizeDelta.y), .1f);
                if (boomCorouOrange != null) { StopCoroutine(boomCorouOrange); healthTextOrange.fontSize = 60; }
                boomCorouOrange = StartCoroutine(SizeBoomText(healthTextOrange, 25f, .008f, .1f));
                break;
        }
        
    }

    public void BloodedHealth(PlayerMovement usedPlr)
    {
        switch (usedPlr.plrType)
        {
            case PlayerTypes.Green:
                TextMeshProUGUI healthTextGreen = green.transform.Find("Health").GetComponent<TextMeshProUGUI>();
                RectTransform healthBarFillGreen = green.transform.Find("Fill").GetComponent<RectTransform>();
                if (plrGreen.stats.health >= 0) { healthTextGreen.text = plrGreen.stats.health.ToString(); }
                else { healthTextGreen.text = 0.ToString(); }
                healthBarFillGreen.DOSizeDelta(new Vector2(plrGreen.stats.health * 4, healthBarFillGreen.sizeDelta.y), .1f);
                if (boomCorouGreen != null) { StopCoroutine(boomCorouGreen); healthTextGreen.fontSize = 60; }
                boomCorouGreen = StartCoroutine(SizeBoomText(healthTextGreen, 25f, .008f, .1f));
                healthTextGreen.color = Color.red;
                healthTextGreen.DOColor(Color.white, .5f);
                break;

            case PlayerTypes.Orange:
                TextMeshProUGUI healthTextOrange = orange.transform.Find("Health").GetComponent<TextMeshProUGUI>();
                RectTransform healthBarFillOrange = orange.transform.Find("Fill").GetComponent<RectTransform>();
                if (plrOrange.stats.health >= 0) { healthTextOrange.text = plrOrange.stats.health.ToString(); }
                else { healthTextOrange.text = 0.ToString(); }
                healthBarFillOrange.DOSizeDelta(new Vector2(plrOrange.stats.health * 4, healthBarFillOrange.sizeDelta.y), .1f);
                if (boomCorouOrange != null) { StopCoroutine(boomCorouOrange); healthTextOrange.fontSize = 60; }
                boomCorouOrange = StartCoroutine(SizeBoomText(healthTextOrange, 25f, .008f, .1f));
                healthTextOrange.color = Color.red;
                healthTextOrange.DOColor(Color.white, .5f);
                break;
        }

    }

    private void UpdateScoreBoard(PlayerMovement wonPlr)
    {
        Transform spawnPoint = null;
        Color color = Color.white;
        switch (wonPlr.plrType)
        {
            case PlayerTypes.Orange:
                Transform boardO = scoreBoard.transform.Find("Orange");
                spawnPoint = boardO.Find(RoundManager.Instance.orangeWonTime.ToString());
                color = orangeHittedColor;
                break;
            case PlayerTypes.Green:
                Transform boardG = scoreBoard.transform.Find("Green");
                spawnPoint = boardG.Find(RoundManager.Instance.greenWonTime.ToString());
                color = greenHittedColor;
                break;
        }

        spawnPoint.GetComponent<Image>().enabled = false;

        GameObject obj = Instantiate(scoreCircle);
        obj.transform.position = spawnPoint.transform.position;
        obj.GetComponent<SpriteRenderer>().color = color;
        obj.transform.SetParent(UI.transform);
    }

    public void RoundEnded(PlayerMovement wonPlr, PlayerMovement lostPlayer)
    {
        UpdateScoreBoard(wonPlr);


        TextMeshProUGUI text = roundEndedText.GetComponent<TextMeshProUGUI>();
        switch (wonPlr.plrType)
        {
            case PlayerTypes.Orange:
                text.color = orangeHittedColor;
                break;
            case PlayerTypes.Green:
                text.color = greenHittedColor;
                break;
        }

        StartCoroutine(SizeBoomText(roundEndedText.GetComponent<TextMeshProUGUI>(), 200f, .5f, .5f));
        roundEndedText.GetComponent<RectTransform>().DOComplete();
        roundEndedText.GetComponent<RectTransform>().DOShakePosition(.5f, 10f, 100);

        StartCoroutine(StartSelection());

        IEnumerator StartSelection()
        {
            yield return new WaitForSeconds(.2f);

            Time.timeScale = 1f;
            ItemManager.Instance.StartPicking(lostPlayer, SelectionUI, skillCards, statCards, passiveCards);
            yield return null;
        }

    }

    public void RoundStarted()
    {
        UpdateHealth(plrOrange);
        UpdateHealth(plrGreen);
    }

    public void GameEnded(PlayerMovement wonPlr)
    {
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            AudioManager.Instance.PlayAudio(reverseSnare);
            yield return new WaitForSeconds(2.5f);

            foreach (GameObject obj in GetChildren(UI))
            {
                obj.SetActive(false);
            }
            roundEndedText.SetActive(true);
            TextMeshProUGUI text = roundEndedText.GetComponent<TextMeshProUGUI>();
            AudioManager.Instance.PlayAudio(winSound);
            GameObject wonPlrSelebrate = null;
            switch (wonPlr.plrType)
            {
                case PlayerTypes.Orange:
                    text.color = orangeHittedColor;
                    wonPlrSelebrate = OrangeWonSprite;
                    break;
                case PlayerTypes.Green:
                    text.color = greenHittedColor;
                    wonPlrSelebrate = GreenWonSprite;
                    break;
            }

            text.text = "game!";
            text.fontSize = 350;
            roundEndedText.GetComponent<RectTransform>().DOComplete();
            roundEndedText.GetComponent<RectTransform>().DOShakeRotation(2f, 10f, 100);
            roundEndedText.GetComponent<RectTransform>().DOShakePosition(2f, 10f, 100);

            yield return new WaitForSeconds(4f);

            wonPlrSelebrate.transform.DOMoveY(-2, 2f);
            text.gameObject.GetComponent<RectTransform>().DOMoveY(3.5f, 2f);
            StartCoroutine(TweenTextSize(text, 200, 4f));


            while (true)
            {
                if (Input.anyKeyDown)
                {
                    AudioClip clip = tauntSounds[Random.Range(0, tauntSounds.Length)];
                    AudioManager.Instance.PlayAudio(clip);
                    yield return new WaitForSeconds(clip.length);
                }
                yield return null;
            }
        }

        IEnumerator TweenTextSize(TextMeshProUGUI text, float size, float duration)
        {
            float elapsedT = 0f;
            while (elapsedT < duration)
            {
                text.fontSize = Mathf.Lerp(text.fontSize, size, elapsedT / duration);
                elapsedT += Time.deltaTime * duration;
                yield return null;
            }
            yield return null;
        }
    }

    private IEnumerator ColorLerp(SpriteRenderer sprite, Color colorA, Color colorB, float duration)
    {
        float elapsedT = 0f;
        while (elapsedT < duration)
        {
            sprite.color = Color.Lerp(colorA, colorB, elapsedT / duration);
            elapsedT += Time.deltaTime * duration;
            yield return null;
        }
        yield return null;
    }

    private IEnumerator SizeBoomText(TextMeshProUGUI text, float AdditionalSize, float holdTime, float duration)
    {
        float originalSize = text.fontSize;
        text.fontSize = originalSize + AdditionalSize;
        yield return new WaitForSeconds(holdTime);

        float elapsedT = 0f;
        while (elapsedT < duration)
        {
            text.fontSize = Mathf.Lerp(text.fontSize, originalSize, elapsedT / duration);
            elapsedT += Time.deltaTime * duration;
            yield return null;
        }
        text.fontSize = originalSize;


        yield return null;
    }

    private GameObject[] GetChildren(GameObject parent)
    {
        GameObject[] children = new GameObject[parent.transform.childCount];

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children[i] = parent.transform.GetChild(i).gameObject;
        }

        return children;
    }

    public void PlayerIced(PlayerMovement plr, List<KeyCode> keys)
    {
        List<string> directionStrings = new List<string>();

        foreach (KeyCode key in keys)
        {
            string direction = GetDirectionString(key);
            directionStrings.Add(direction);
        }

        string result = "Iced!: " + string.Join(", ", directionStrings);

        switch (plr.plrType)
        {
            case PlayerTypes.Orange:
                orange.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().text = result;
                break;
            case PlayerTypes.Green:
                green.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().text = result;
                break;
        }

        string GetDirectionString(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.W:
                case KeyCode.UpArrow:
                    return "À§";
                case KeyCode.A:
                case KeyCode.LeftArrow:
                    return "¿Þ";
                case KeyCode.S:
                case KeyCode.DownArrow:
                    return "¾Æ·¡";
                case KeyCode.D:
                case KeyCode.RightArrow:
                    return "¿À¸¥";
                default:
                    return "";
            }
        }
    }

    public void PlayerDeIced(PlayerMovement plr)
    {
        switch (plr.plrType)
        {
            case PlayerTypes.Orange:
                orange.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().text = "";
                break;
            case PlayerTypes.Green:
                green.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().text = "";
                break;
        }
    }

    public void PlayerIceWrong(PlayerMovement plr)
    {
        StartCoroutine(enumer());

        IEnumerator enumer()
        {
            switch (plr.plrType)
            {
                case PlayerTypes.Orange:
                    orange.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().DOComplete();
                    orange.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().color = new Color(0, 0.16f, 1, 0.7f);
                    orange.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().DOColor(new Color(0, 1, 1, 0.7f), .1f);
                    break;
                case PlayerTypes.Green:
                    green.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().DOComplete();
                    green.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().color = new Color(0, 0.16f, 1, 0.7f);
                    green.transform.Find("InputKey").GetComponent<TextMeshProUGUI>().DOColor(new Color(0, 1, 1, 0.7f), .1f);
                    break;
            }
            yield return null;
        }
    }


}
