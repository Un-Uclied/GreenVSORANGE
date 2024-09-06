using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using static UnityEngine.ParticleSystem;

public class IceBergSkillScript : MonoBehaviour
{
    GameObject OwnedPlayer;
    public int skillNumber = 2;
    [SerializeField] GameObject icon;
    [SerializeField] public SkillType type = SkillType.IceBerg;
    [SerializeField] PolygonCollider2D hitRange;
    [SerializeField] SpriteRenderer warningRange;
    [SerializeField] ParticleSystem particle;

    [Header("stats")]
    [SerializeField] public bool onCoolTime;
    [SerializeField] public float coolTime;
    [SerializeField] public float prepareTime;
    [SerializeField] public int damage;
    [SerializeField] public bool isAttacking;
    [SerializeField] public float duration;

    [Header("Sounds")]
    [SerializeField] private AudioClip useAudio;

    private Coroutine enumer;
    public int keyLength = 3;

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
        icon.transform.position = plr.UISector.transform.Find($"Skill{skillNumber}").transform.position;
        icon.transform.SetParent(plr.UISector.transform.Find($"Skill{skillNumber}").transform);

        plr.plrSkillSecond += OnAttack;
        plr.plrGetsSkill += OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted += OnRoundStart;

        hitRange.enabled = false;
        warningRange.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        plr.plrSkillSecond -= OnAttack;
        plr.plrGetsSkill -= OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted -= OnRoundStart;
    }

    private void Update()
    {
        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();

        transform.position = OwnedPlayer.transform.position;
        if (plr.stats.isAnchored) { return; }
        switch (plr.stats.watchingDir)
        {
            case WatchingDirection.Left:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case WatchingDirection.LeftDown:
                transform.rotation = Quaternion.Euler(0, 0, 45);
                break;
            case WatchingDirection.LeftUp:
                transform.rotation = Quaternion.Euler(0, 0, -45);
                break;
            case WatchingDirection.Right:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case WatchingDirection.RightUp:
                transform.rotation = Quaternion.Euler(0, 0, -1325);
                break;
            case WatchingDirection.RightDown:
                transform.rotation = Quaternion.Euler(0, 0, 125);
                break;
            case WatchingDirection.FlipedUp:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case WatchingDirection.FlipedDown:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case WatchingDirection.Up:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case WatchingDirection.Down:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform.name != OwnedPlayer.name)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }

            PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
            if (hittedPlr.stats.isIceBerged) { return; }
            hittedPlr.ChangeHealth(hittedPlr.stats.health - damage);
            UIScript.Instance.OnHit(hittedPlr);
            UIScript.Instance.UpdateHealth(hittedPlr);

            StartCoroutine(OnHit(hittedPlr));
        }
    }

    public void OnAttack(/*InputAction.CallbackContext context*/)
    {
        //if (!context.started) { return; }
        if (isAttacking) { return; }
        if (OwnedPlayer.GetComponent<PlayerMovement>().stats.isAnchored) { return; }

        AudioManager.Instance.PlayAudio(useAudio);

        StartCoroutine(enumerator());


        IEnumerator enumerator()
        {
            isAttacking = true;
            
            hitRange.enabled = false;
            warningRange.enabled = true;
            PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
            switch (plr.plrType)
            {
                case PlayerTypes.Orange:
                    warningRange.color = AttackHandler.Instance.offMatOrange.color;
                    break;
                case PlayerTypes.Green:
                    warningRange.color = AttackHandler.Instance.offMatGreen.color;
                    break;
            }

            yield return new WaitForSeconds(prepareTime);
            particle.Play();
            hitRange.enabled = true;
            warningRange.enabled = true;
            warningRange.color = plr.color;

            yield return new WaitForSeconds(duration);

            hitRange.enabled = false;
            warningRange.enabled = false;
            isAttacking = false;
            enumer = StartCoroutine(ApplyCoolTime());

            yield return null;
        }
    }

    public void OnRoundStart()
    {
        icon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        if (enumer != null) { StopCoroutine(enumer); }
        onCoolTime = false;
    }

    public bool OnPlayerGetsUpgrade(SkillCards selectedCard)
    {
        if (skillNumber == selectedCard.skillNumber && name == selectedCard.transform.name)
        {
            damage += 5;
            keyLength++;
            return true;
        }
        else
        {
            if (skillNumber != selectedCard.skillNumber) { return false; }
            else { Destroy(gameObject); return false; }
        }
    }

    IEnumerator OnHit(PlayerMovement hittedPlr)
    {
        hittedPlr.SetIced(keyLength);
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
}
