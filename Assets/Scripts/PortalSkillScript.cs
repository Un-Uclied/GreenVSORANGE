using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PortalSkillScript : MonoBehaviour
{
    public int skillNumber = 1; // 1 or 2
    public GameObject OwnedPlayer;
    [SerializeField] CircleCollider2D hitRange;
    [SerializeField] SpriteRenderer warningRange;
    [SerializeField] SpriteRenderer portalSprite;
    [SerializeField] GameObject icon;
    [SerializeField] public SkillType type = SkillType.Dash;


    [Header("stats")]
    [SerializeField] public bool onCoolTime;
    [SerializeField] public float coolTime;
    [SerializeField] public float prepareTime;
    [SerializeField] public int damage;
    [SerializeField] public float duration;
    [SerializeField] public bool isAttacking;

    [Header("Sounds")]
    [SerializeField] private AudioClip startAudio;
    [SerializeField] private AudioClip usedAudio;

    private Coroutine enumer;

    private void Awake()
    {
        name = type.ToString();
    }


    private void Start()
    {
        portalSprite.transform.localScale = Vector3.zero;
        float distanceOrange = Vector3.Distance(transform.position, ItemManager.Instance.playerOrange.transform.position);
        float distanceGreen = Vector3.Distance(transform.position, ItemManager.Instance.playerGreen.transform.position);

        OwnedPlayer = (distanceOrange < distanceGreen) ? ItemManager.Instance.playerOrange : ItemManager.Instance.playerGreen;

        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        plr.plrSkillFirst += OnAttack;
        plr.plrGetsSkill += OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted += OnRoundStart;
        hitRange.enabled = false;

        icon.transform.position = plr.UISector.transform.Find($"Skill{skillNumber}").transform.position;
        icon.transform.SetParent(plr.UISector.transform.Find($"Skill{skillNumber}").transform);

        switch (plr.plrType)
        {
            case PlayerTypes.Orange:
                warningRange.color = AttackHandler.Instance.offMatOrange.color;
                break;
            case PlayerTypes.Green:
                warningRange.color = AttackHandler.Instance.offMatGreen.color;
                break;
        }
        warningRange.enabled = false;
    }

    private void OnDestroy()
    {
        Destroy(icon);
        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        plr.plrSkillFirst -= OnAttack;
        plr.plrGetsSkill -= OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted -= OnRoundStart;
    }

    public void OnAttack(/*InputAction.CallbackContext context*/)
    {

        if (onCoolTime) { return; }
        //if (!context.started) { return; }
        if (OwnedPlayer.GetComponent<PlayerMovement>().stats.isAnchored) { return; }
        AudioManager.Instance.PlayAudio(startAudio);

        portalSprite.transform.localScale = Vector3.zero;
        onCoolTime = true;
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            warningRange.enabled = true;
            portalSprite.transform.DOScale(1, prepareTime);
            yield return new WaitForSeconds(prepareTime);
            PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
            switch (plr.plrType)
            {
                case PlayerTypes.Orange:
                    warningRange.color = AttackHandler.Instance.onMatOrange.color;
                    break;
                case PlayerTypes.Green:
                    warningRange.color = AttackHandler.Instance.onMatGreen.color;
                    break;
            }
            hitRange.enabled = true;
            isAttacking = true;
            AudioManager.Instance.PlayAudio(usedAudio);

            portalSprite.transform.DOScale(0, duration);
            yield return new WaitForSeconds(duration);

            hitRange.enabled = false;
            warningRange.enabled = false;
            isAttacking = false;
            plr.stats.isInvincible = true;
            switch (plr.plrType)
            {
                case PlayerTypes.Orange:
                    warningRange.color = AttackHandler.Instance.offMatOrange.color;
                    OwnedPlayer.transform.Find("CharacterSprite").DOScale(0, .1f);
                    OwnedPlayer.transform.DOMove(RoundManager.Instance.orangeStartPos, .1f);
                    yield return new WaitForSeconds(.1f);
                    OwnedPlayer.transform.Find("CharacterSprite").localScale = new Vector3(.12f, .12f, .12f);
                    break;
                case PlayerTypes.Green:
                    warningRange.color = AttackHandler.Instance.offMatGreen.color;
                    
                    OwnedPlayer.transform.DOMove(RoundManager.Instance.greenStartPos, .1f);
                    yield return new WaitForSeconds(.1f);
                    OwnedPlayer.transform.Find("CharacterSprite").localScale = new Vector3(.12f, .12f, .12f);
                    break;
            }

            yield return new WaitForSeconds(.15f);
            plr.stats.isInvincible = false;

            StartCoroutine(ApplyCoolTime());
        }
    }

    private void Update()
    {
        portalSprite.transform.Rotate(new Vector3(0, 0, 1500f * Time.deltaTime));
        if (isAttacking) { return; }
        transform.position = OwnedPlayer.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform.name != OwnedPlayer.name)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }

            PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
            if (hittedPlr.stats.isConfused) { return; }
            hittedPlr.ChangeHealth(hittedPlr.stats.health - damage);
            UIScript.Instance.OnHit(hittedPlr);
            UIScript.Instance.UpdateHealth(hittedPlr);
        }
    }
    IEnumerator ApplyCoolTime()
    {
        icon.GetComponent<SpriteRenderer>().color = new Color(.2f, .2f, .2f, 1f);
        yield return new WaitForSeconds(coolTime);
        onCoolTime = false;
        isAttacking = false;
        icon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        yield return null;
    }

    public bool OnPlayerGetsUpgrade(SkillCards selectedCard)
    {
        if (skillNumber == selectedCard.skillNumber && name == selectedCard.transform.name)
        {
            prepareTime -= .25f;
            damage += 7;
            return true;
        }
        else 
        {
            if (skillNumber != selectedCard.skillNumber) { return false; }
            else { Destroy(gameObject); return false; }
        }

    }

    public void OnRoundStart()
    {
        icon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        if (enumer != null) { StopCoroutine(enumer); }
        onCoolTime = false;
    }
}
