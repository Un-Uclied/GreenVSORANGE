using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class DashSkillScript : MonoBehaviour
{
    public int skillNumber = 2; // 1 or 2
    public GameObject OwnedPlayer;
    [SerializeField] GameObject lightObject;
    [SerializeField] TrailRenderer trail;
    [SerializeField] CircleCollider2D hitRange;
    [SerializeField] GameObject icon;
    [SerializeField] public SkillType type = SkillType.Dash;


    [Header("stats")]
    [SerializeField] public bool onCoolTime;
    [SerializeField] public float coolTime;
    [SerializeField] public float prepareTime;
    [SerializeField] public int damage;
    [SerializeField] public bool isAttacking;
    private Coroutine enumer;

    [Header("Sounds")]
    [SerializeField] private AudioClip useAudio;

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
        trail.startColor = plr.color;
        trail.endColor = plr.color;
        plr.plrSkillSecond += OnAttack;
        plr.plrGetsSkill += OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted += OnRoundStart;
        hitRange.enabled = false;
        trail.enabled = false;
        lightObject.SetActive(false);
        lightObject.GetComponent<Light2D>().color = plr.color;

        icon.transform.position = plr.UISector.transform.Find($"Skill{skillNumber}").transform.position;
        icon.transform.SetParent(plr.UISector.transform.Find($"Skill{skillNumber}").transform);
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
        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        AudioManager.Instance.PlayAudio(useAudio);

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            Vector3 originalVelocity = plr.GetComponent<Rigidbody2D>().velocity;
            plr.GetComponent<Rigidbody2D>().velocity = plr.moveDirection * 22.0f;
            isAttacking = true;
            trail.enabled = true;
            hitRange.enabled = true;
            lightObject.SetActive(true);

            yield return new WaitForSeconds(.2f);

            // �뽬 ����: ������ �ӵ��� ����
            hitRange.enabled = false;
            plr.GetComponent<Rigidbody2D>().velocity = originalVelocity;
            plr.stats.isInvincible = false;
            isAttacking = false;
            trail.enabled = false;
            lightObject.SetActive(false);
            

            StartCoroutine(ApplyCoolTime());
            yield return null;
        }
        
        
    }

    private void Update()
    {
        transform.position = OwnedPlayer.transform.position;
        lightObject.transform.Rotate(new Vector3(0,0, 650f * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform.name != OwnedPlayer.name)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }

            PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
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
