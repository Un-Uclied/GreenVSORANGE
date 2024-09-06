using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BladeSkillScript : MonoBehaviour
{
    public int damage;

    GameObject OwnedPlayer;
    public int skillNumber = 1; // 1 or 2
    [SerializeField] GameObject icon;
    public int bladeRemaining = 3;
    [SerializeField] public SkillType type = SkillType.Blade;
    [SerializeField] private GameObject bladePrefab;

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
        icon.transform.position = plr.UISector.transform.Find($"Skill{skillNumber}").transform.position;
        icon.transform.SetParent(plr.UISector.transform.Find($"Skill{skillNumber}").transform);

        plr.plrSkillFirst += OnAttack;
        plr.plrGetsSkill += OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted += OnRoundStart;
    }

    private void OnDestroy()
    {
        PlayerMovement plr = OwnedPlayer.GetComponent<PlayerMovement>();
        plr.plrSkillFirst -= OnAttack;
        plr.plrGetsSkill -= OnPlayerGetsUpgrade;
        RoundManager.Instance.OnRoundStarted -= OnRoundStart;
    }

    public void OnAttack(/*InputAction.CallbackContext context*/)
    {
        if (bladeRemaining == 0) { icon.GetComponent<SpriteRenderer>().color = new Color(.2f, .2f, .2f, 1f); return; }
        //if (!context.started) { return; }
        if (OwnedPlayer.GetComponent<PlayerMovement>().stats.isAnchored) { return; }

        StartCoroutine(enumerator());

        bladeRemaining -= 1;
        AudioManager.Instance.PlayAudio(useAudio);

        IEnumerator enumerator()
        {
            GameObject spawnedBlade = Instantiate(bladePrefab);
            spawnedBlade.transform.position = OwnedPlayer.transform.position;
            BladeScript bladeScript = spawnedBlade.GetComponent<BladeScript>();
            bladeScript.OwningPlayer = OwnedPlayer;
            spawnedBlade.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = OwnedPlayer.GetComponent<PlayerMovement>().color;
            bladeScript.damage = damage;

            spawnedBlade.transform.SetParent(GameObject.Find("Obstacles").transform);
            yield return null; 
        }
    }

    public void OnRoundStart()
    {
        bladeRemaining = 3;
        icon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
    }

    public bool OnPlayerGetsUpgrade(SkillCards selectedCard)
    {
        if (skillNumber == selectedCard.skillNumber && name == selectedCard.transform.name)
        {
            damage += 5;
            return true;
        }
        else
        {
            if (skillNumber != selectedCard.skillNumber) { return false; }
            else { Destroy(gameObject); return false; }
        }
    }
}
