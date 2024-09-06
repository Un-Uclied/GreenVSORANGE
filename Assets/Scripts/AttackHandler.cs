using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwordTypes
{
    Default,
    Wind,
    Blood,
    Long,
    Chakram
}

public class AttackHandler : MonoBehaviour
{
    [Header("Colors")]
    public Material onMatOrange;
    public Material offMatOrange;
    public Material onMatGreen;
    public Material offMatGreen;
    [Header("Prefabs")]
    [SerializeField] private GameObject windPrefab;
    [Header("Sounds")]
    [SerializeField] private AudioClip attackSound;


    private static AttackHandler instance;

    public static AttackHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AttackHandler>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("AttackHandler");
                    instance = obj.AddComponent<AttackHandler>();
                }
            }

            return instance;
        }
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
    }

    public void OnHitted(GameObject plrAttacked, GameObject plrGotHitted, SwordScript sword)
    {
        PlayerMovement attackPlr = plrAttacked.GetComponent<PlayerMovement>();
        PlayerMovement hittedPlr = plrGotHitted.GetComponent<PlayerMovement>();

        UIScript.Instance.UpdateHealth(hittedPlr);
        UIScript.Instance.OnHit(hittedPlr);
    }

    public void Attack(PlayerMovement plr, SwordScript sword)
    {
        AudioManager.Instance.PlayAudio(attackSound);
        if (sword.swordType == SwordTypes.Wind)
        {
            MakeWind(plr, sword);
        }
        else if (sword.swordType == SwordTypes.Blood)
        {
            HandleBlood(plr, sword);
        }
    }

    private void MakeWind(PlayerMovement plr, SwordScript sword)
    {
        GameObject wind = Instantiate(windPrefab);
        wind.transform.position = plr.transform.position;
        Vector2 dir = Vector2.zero;
        switch (plr.stats.watchingDir)
        {
            case WatchingDirection.Left:
                dir = Vector2.left;
                break;
            case WatchingDirection.Right:
                dir = Vector2.right;
                break;
            case WatchingDirection.Up:
                dir = Vector2.up;
                break;
            case WatchingDirection.Down:
                dir = Vector2.down;
                break;
            case WatchingDirection.FlipedDown:
                dir = Vector2.down;
                break;
            case WatchingDirection.FlipedUp:
                dir = Vector2.up;
                break;
            case WatchingDirection.LeftDown:
                dir = Vector2.left + Vector2.down;
                break;
            case WatchingDirection.RightDown:
                dir = Vector2.right + Vector2.down;
                break;
            case WatchingDirection.LeftUp:
                dir = Vector2.left + Vector2.up;
                break;
            case WatchingDirection.RightUp:
                dir = Vector2.right + Vector2.up;
                break;
        }
        StartCoroutine(MoveWind());

        Wind windScpt = wind.GetComponent<Wind>();
        windScpt.damage = 15 + plr.extraStats.extraAttackDamage;

        wind.transform.Find("Sprite").rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg);

        IEnumerator MoveWind()
        {
            float despawn = 0;
            while(despawn < 3.5f)
            {
                wind.transform.Translate(dir.normalized * 20f * Time.deltaTime);
                despawn += Time.deltaTime;
                yield return null;
            }
            Destroy(wind);
            yield return null;
        }
    }

    private void HandleBlood(PlayerMovement plr, SwordScript sword)
    {
        if (plr.stats.health > 15)
        {
            sword.stats.attackDamage = sword.module.attackDamage + plr.extraStats.extraAttackDamage;
            plr.ChangeHealth(plr.stats.health - 15);
            UIScript.Instance.BloodedHealth(plr);
        }
        else
        {
            sword.stats.attackDamage = 10;
        }
    }
}
