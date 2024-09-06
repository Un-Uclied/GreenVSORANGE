using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardTypes
{
    Skill = KeyCode.Alpha1,
    Stat = KeyCode.Alpha2,
    Passive = KeyCode.Alpha3
}

public class SkillCards : MonoBehaviour
{
    public GameObject icon;

    public CardTypes type;

    public bool canSelect = false;

    [HideInInspector] public Vector3 originalPos = Vector3.zero;

    public GameObject skillManager;

    private KeyCode keyOrange = KeyCode.Alpha0;
    private KeyCode keyGreen = KeyCode.Alpha0;
    public int skillNumber;

    [Header("Sounds")]
    [SerializeField] private AudioClip infoSound;

    private void OnMouseDown()
    {
        if (canSelect)
        {
            ItemManager.Instance.OnCardClick(this);
        }
    }

    private void Start()
    {
        switch (type)
        {
            case CardTypes.Skill:
                keyOrange = KeyCode.Alpha1;
                keyGreen = KeyCode.L;
                break;
            case CardTypes.Stat:
                keyOrange = KeyCode.Alpha2;
                keyGreen = KeyCode.Semicolon;
                break;
            case CardTypes.Passive:
                keyOrange = KeyCode.Alpha3;
                keyGreen = KeyCode.Quote;
                break;
        }
    }

    private void Update()
    {
        if (canSelect)
        {
            if (Input.GetKeyDown(keyOrange) || Input.GetKeyDown(keyGreen))
            {
                AudioManager.Instance.PlayAudio(infoSound);
                transform.DOMove(new Vector3(0, 0, -6), 1f);
                transform.Find("Sprite").DOScale(new Vector3(.85f, .85f, .85f), 1f);
            }
            if (Input.GetKeyUp((KeyCode)type) || Input.GetKeyUp(keyGreen)) 
            {
                transform.DOMove(originalPos, 1f);
                transform.Find("Sprite").DOScale(new Vector3(.45f, .45f, .45f), 1f);
            }
        }
    }
}
