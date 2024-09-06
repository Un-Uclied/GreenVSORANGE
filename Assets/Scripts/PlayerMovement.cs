using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerTypes
{
    Orange,
    Green
}

public enum WatchingDirection
{
    FlipedUp, FlipedDown,
    Up, Down,
    RightUp, RightDown,
    LeftUp, LeftDown,
    Right, Left,
}

public struct ExtraStats
{
    public float extraWalkSpeed;
    public int extraAttackDamage;
    public float extraAttackSpeed;
    public int extraHealAmount;
    public float extraHealSpeed;
    public int extraHealth;
}

public struct PlayerStats
{
    public float walkSpeed;
    public int health;
    public int maxHealth;
    public float healSpeed;
    public int healAmount;

    public bool isAnchored;
    public bool isConfused;

    public WatchingDirection watchingDir;

    public GameObject Skill1;
    public GameObject Skill2;

    public bool isInvincible;
    public bool isDashing;
    public bool isIceBerged;

    public float lastestHitTime;
}

public class PlayerMovement : MonoBehaviour
{
    [Header("InputReferences")]
    [SerializeField] private VariableJoystick joystick;
    [Space]
    [SerializeField] CharacterController controller;
    [SerializeField] public GameObject sprite;
    [SerializeField] public PlayerTypes plrType;
    [SerializeField] public GameObject swordGameObject;
    [SerializeField] public Color color;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] public GameObject UISector;
    [SerializeField] private AudioClip hitSound;

    public delegate void PlayerSkillAction();
    public PlayerSkillAction plrSkillFirst;
    public PlayerSkillAction plrSkillSecond;
    public delegate bool PlayerGetsUpgrade(SkillCards selectedCard);
    public PlayerGetsUpgrade plrGetsSkill;
    public delegate void PlayerStatusChanged(PlayerMovement plr);
    public PlayerStatusChanged plrDied;

    public Vector3 moveDirection = Vector3.zero;
    

    public PlayerStats stats;
    public ExtraStats extraStats;

    private void Awake()
    {
        // ���� �̴ϼȶ�����
        stats.health = 100;
        stats.maxHealth = 100;
        stats.walkSpeed = 4f;
        stats.isAnchored = false;
        stats.healAmount = 2;
        stats.healSpeed = 1f;

        
        StartCoroutine(AutoHealPlayer());
    }
    private void Start()
    {
        RoundManager.Instance.OnRoundStarted += OnRoundStart;
        RoundManager.Instance.OnRoundEnded += OnRoundEnded;
    }

    public void ChangeHealth(int value)
    {
        if (stats.health <= 0) { return; }
        if (stats.isInvincible) { return; }

        if (value > stats.health)
        {
            stats.health = value;
            if (stats.health > 100) { stats.health = 100; }
            UIScript.Instance.UpdateHealth(this);
        }
        else
        {
            stats.health = value;
            HitParticle();
            AudioManager.Instance.PlayAudio(hitSound);

            stats.lastestHitTime = Time.time;
            stats.isInvincible = true;
            StartCoroutine(enumerator());
        }
        if (stats.health <= 0)
        {
            RoundManager.Instance.PlayerDies(this);
            sprite.SetActive(false);
            transform.Find("Effects").gameObject.SetActive(false);
            plrDied?.Invoke(this);
        }

        IEnumerator enumerator()
        {
            yield return new WaitForSeconds(.1f);
            stats.isInvincible = false;
            yield return null;
        }
    }

    private void Update()
    {
        if (stats.isAnchored) { return; }
        MoveInput();
        PlayerMove();
    }

    private void PlayerChangeSprite()
    {
        if (moveDirection == Vector3.zero) { return; }
        if (stats.isAnchored) { return; }
        
        SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
        float target = 0f;

        if (moveDirection.y > 0)
        {
            if (renderer.flipX)
            {
                target = 50;
                stats.watchingDir = WatchingDirection.FlipedUp;
            } 
            else
            {
                target = -50;
                stats.watchingDir = WatchingDirection.Up;
            }
                
        }
        else
        {
            if (renderer.flipX)
            {
                target = -50;
                stats.watchingDir = WatchingDirection.FlipedDown;
            }
            else
            {
                target = 50;
                stats.watchingDir = WatchingDirection.Down;
            }
                
        }

        if (moveDirection.x == 1)
        {
            renderer.flipX = true;
            target = 0;
            stats.watchingDir = WatchingDirection.Right;
        }
        else if (moveDirection.x == -1)
        {
            renderer.flipX = false;
            target = 0;
            stats.watchingDir = WatchingDirection.Left;
        }

        if (moveDirection.x != 1 && moveDirection.x != 0)
        {
            //����
            if (moveDirection.y > 0f)
            {
                if (moveDirection.x > 0f)
                {
                    renderer.flipX = true;
                    target = 30;
                    stats.watchingDir = WatchingDirection.RightUp;
                }
                else
                {
                    renderer.flipX = false;
                    target = -30;
                    stats.watchingDir = WatchingDirection.LeftUp;
                }
            }
            //�Ʒ���
            else if (moveDirection.y < 0f)
            {
                if (moveDirection.x > 0f)
                {
                    renderer.flipX = true;
                    target = -30;
                    stats.watchingDir = WatchingDirection.RightDown;
                }
                else
                {
                    renderer.flipX = false;
                    target = 30;
                    stats.watchingDir = WatchingDirection.LeftDown;
                }
            }
        }

       
        sprite.transform.DORotateQuaternion(Quaternion.Euler(0, 0, target), .25f);
    }

    private void PlayerMove()
    {
        transform.Translate(moveDirection * (stats.walkSpeed + extraStats.extraWalkSpeed) * Time.deltaTime);
        
        // ȭ�� ������ ������ �ʵ��� ��ġ ����
        float screenWidth = Camera.main.orthographicSize * Screen.width / Screen.height;
        float screenHeight = Camera.main.orthographicSize;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -screenWidth, screenWidth);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -screenHeight, screenHeight);

        transform.position = clampedPos;
    }

    IEnumerator AutoHealPlayer()
    {
        while (true)
        {
            
            yield return new WaitForSeconds(stats.healSpeed - extraStats.extraHealSpeed);

            if (Time.time - stats.lastestHitTime >= 4.2f && stats.health < 100)
            {
                ChangeHealth(stats.health + stats.healAmount + extraStats.extraHealAmount);
            }
            yield return null;
        }
    }

    public void FirstSkill()
    {
        plrSkillFirst?.Invoke();
    }

    public void SecondSkill()
    {
        plrSkillSecond?.Invoke();
    }

    public void MoveInput()
    {
        // PC
        //Vector2 moveVector = context.ReadValue<Vector2>();
        Vector2 moveVector = joystick.Direction;
        moveVector = new Vector2(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));
        if (stats.isConfused == false) { moveDirection = new Vector3(moveVector.x, moveVector.y, 0f).normalized; }
        else { moveDirection = new Vector3(moveVector.y, moveVector.x, 0f).normalized; }

        PlayerChangeSprite();

        SwordScript sword = swordGameObject.GetComponent<SwordScript>();
        sword.RotateCollider();
        //if (context.canceled) { return; }
        if (moveVector.magnitude == 0) { return; }
        sword.SwordPosition();
    }

    private void OnRoundEnded(PlayerMovement wonPlr, PlayerMovement lostPlayer)
    {
        stats.isAnchored = true;
    }

    private void OnRoundStart()
    {
        stats.isAnchored = false;
        stats.isIceBerged = false;
        UIScript.Instance.PlayerDeIced(this);

        sprite.SetActive(true);
        transform.Find("Effects").gameObject.SetActive(true);
        stats.health = stats.maxHealth;
        UIScript.Instance.UpdateHealth(this);
    }

    private void HitParticle()
    {
        GameObject hit = Instantiate(hitParticle);
        hit.GetComponent<ParticleSystem>().startColor = color;
        hit.transform.position = transform.position;
        StartCoroutine(DestroyParticle(hit));

        IEnumerator DestroyParticle(GameObject particleObject)
        {
            Destroy(particleObject, 1f);
            yield return null;
        }
    }

    public void GiveSkill(SkillCards selectedCard)
    {
        var gotUpgraded = plrGetsSkill?.Invoke(selectedCard);

        gotUpgraded ??= false;
        if ((bool)gotUpgraded == false)
        {
            GameObject skillObject = Instantiate(selectedCard.skillManager);
            skillObject.transform.position = transform.position;
        }
    }

    public void GiveStats(SkillCards selectedCard)
    {
        switch (selectedCard.name)
        {
            case "Health":
                extraStats.extraHealth += 20;
                stats.maxHealth = stats.maxHealth + extraStats.extraHealth;
                break;
            case "Sharpness":
                extraStats.extraAttackSpeed += .2f;
                break;
            case "SharpBlade":
                extraStats.extraAttackDamage += 3;
                break;
            case "Heal":
                extraStats.extraHealAmount += 1;
                extraStats.extraHealSpeed += .25f;
                break;
            case "FasterShoes":
                extraStats.extraWalkSpeed += .8f;
                break;
            default: Debug.Log("Error! Envalid Card!"); break;
        }
    }

    public void SetIced(int length)
    {
        KeyCode[] arrowKeysForGreen = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow }; ;
        KeyCode[] arrowKeysForOrange = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };

        stats.isIceBerged = true;
        stats.isAnchored = true;
        List<KeyCode> keys = new List<KeyCode>();

        for (int i = 0; i < length; i++)
        {
            keys.Add(GetRandomArrowKey());
        }

        UIScript.Instance.PlayerIced(this, keys);

        StartCoroutine(enumerator());

        IEnumerator enumerator()
        {
            while (stats.isIceBerged)
            {
                if (keys.Count <= 0)
                {
                    stats.isIceBerged = false;
                    stats.isAnchored = false;
                    UIScript.Instance.PlayerDeIced(this);
                    break; 
                }
                
                if (Input.GetKeyDown(keys[0]))
                {
                    keys.RemoveAt(0);
                    UIScript.Instance.PlayerIced(this, keys);
                }
                else
                {
                    if (Input.anyKeyDown)
                    {
                        if (plrType == PlayerTypes.Orange)
                        {
                            foreach (KeyCode keyCode in arrowKeysForOrange)
                            {
                                if (Input.GetKey(keyCode))
                                {
                                    UIScript.Instance.PlayerIceWrong(this);
                                    yield return new WaitForSeconds(0.25f);
                                }
                            }
                        }
                        else
                        {
                            foreach (KeyCode keyCode in arrowKeysForGreen)
                            {
                                if (Input.GetKey(keyCode))
                                {
                                    UIScript.Instance.PlayerIceWrong(this);
                                    yield return new WaitForSeconds(0.25f);
                                }
                            }
                        }
                        
                    }

                }
                yield return null;
            }
            yield return null;
        }

        KeyCode GetRandomArrowKey()
        {
            

            switch (plrType)
            {
                case PlayerTypes.Orange:
                    return arrowKeysForOrange[Random.Range(0, arrowKeysForOrange.Length)];
                case PlayerTypes.Green:
                    return arrowKeysForGreen[Random.Range(0, arrowKeysForGreen.Length)];
                
                default: return arrowKeysForOrange[Random.Range(0, arrowKeysForOrange.Length)];
            }
        }
    }
}
