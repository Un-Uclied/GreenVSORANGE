using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SkillType
{
    Tornado,
    Dash,
    Blade,
    IceBerg,
    Portal,

}

public class TornadoScript : MonoBehaviour
{
    public int skillNumber; // 1 or 2
    public GameObject OwnedPlayer;
    [SerializeField] GameObject sprite;
    [SerializeField] CircleCollider2D hitRange;
    [SerializeField] SpriteRenderer warningRange;
    [SerializeField] ParticleSystem particle;
    [SerializeField] GameObject icon;
    [SerializeField] public SkillType type = SkillType.Tornado;
    

    [Header("stats")]
    [SerializeField] public bool onCoolTime;
    [SerializeField] public float coolTime;
    [SerializeField] public float prepareTime;
    [SerializeField] public int damage;
    [SerializeField] public float duration;
    [SerializeField] public bool isAttacking;
    [SerializeField] public float confusedTime;

    [Header("Audio")]
    [SerializeField] private AudioClip useAudio;

    private Coroutine enumer;

    private void Awake()
    {
        name = type.ToString();
    }


    private void Start()
    {
        float distanceOrange = Vector3.Distance(transform.position, ItemManager.Instance.playerOrange.transform.position);
        float distanceGreen = Vector3.Distance(transform.position, ItemManager.Instance.playerGreen.transform.position);

        OwnedPlayer = (distanceOrange < distanceGreen) ? ItemManager.Instance.playerOrange : ItemManager.Instance.playerGreen;

        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        plr.plrSkillFirst += OnAttack;
        plr.plrGetsSkill += OnPlayerGetsUpgrade;
        sprite.transform.localScale = new Vector3(0, 0, 0);
        sprite.GetComponent<SpriteRenderer>().enabled = false;
        sprite.GetComponent<SpriteRenderer>().color = plr.color;
        hitRange.enabled = false;

        icon.transform.position = plr.UISector.transform.Find($"Skill{skillNumber}").transform.position;
        icon.transform.SetParent(plr.UISector.transform.Find($"Skill{skillNumber}").transform);
        RoundManager.Instance.OnRoundStarted += OnRoundStart;


        switch (plr.plrType)
        {
            case PlayerTypes.Orange:
                warningRange.color = AttackHandler.Instance.offMatOrange.color;
                particle.startColor = AttackHandler.Instance.onMatOrange.color;
                sprite.GetComponent<SpriteRenderer>().color = AttackHandler.Instance.onMatOrange.color;
                break;
            case PlayerTypes.Green:
                warningRange.color = AttackHandler.Instance.offMatGreen.color;
                particle.startColor = AttackHandler.Instance.onMatGreen.color;
                sprite.GetComponent<SpriteRenderer>().color = AttackHandler.Instance.onMatGreen.color;
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

        onCoolTime = true;
        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            sprite.GetComponent<SpriteRenderer>().enabled = true;
            
            warningRange.enabled = true;

            yield return new WaitForSeconds(prepareTime);
            
            hitRange.enabled = true;
            AudioManager.Instance.PlayAudio(useAudio);
            isAttacking = true;
            particle.Play();
            sprite.transform.DOScale(new Vector3(.75f, .75f, .75f), .4f);
            StartCoroutine(time());

            while (isAttacking)
            {
                sprite.transform.Rotate(new Vector3(0, 0, 1000f * Time.deltaTime));
                yield return null;
            }
            
            hitRange.enabled = false;
            warningRange.enabled = false;
            StartCoroutine(ApplyCoolTime());

            sprite.transform.DOScale(new Vector3(0, 0, 0), .3f);
            yield return new WaitForSeconds(.4f);
            sprite.GetComponent<SpriteRenderer>().enabled = false;
        }

        IEnumerator time()
        {
            yield return new WaitForSeconds(duration);
            isAttacking = false;
            yield return null;
        }
    }

    private void Update()
    {
        if (isAttacking || sprite.GetComponent<SpriteRenderer>().enabled == true) { return; }
        transform.position = OwnedPlayer.transform.position;
    }

    public void OnRoundStart()
    {
        icon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        if (enumer != null) { StopCoroutine(enumer); }
        onCoolTime = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform.name != OwnedPlayer.name)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }

            PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
            if ( hittedPlr.stats.isConfused) { return; }
            hittedPlr.ChangeHealth(hittedPlr.stats.health - damage);
            UIScript.Instance.OnHit(hittedPlr);
            UIScript.Instance.UpdateHealth(hittedPlr);

            StartCoroutine(OnHit(collision));
        }
    }

    IEnumerator OnHit(Collider2D collision)
    {
        collision.transform.GetComponent<PlayerMovement>().stats.isConfused = true;
        yield return new WaitForSeconds(confusedTime);
        collision.transform.GetComponent<PlayerMovement>().stats.isConfused = false;
        yield return null;
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
            confusedTime += 1f;
            prepareTime -= .05f;
            damage += 7;
            return true;
        }
        else
        {
            if (skillNumber != selectedCard.skillNumber) { return false; }
            else { Destroy(gameObject); return false; }
        }
    }
}
