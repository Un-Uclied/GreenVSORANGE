using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeScript : MonoBehaviour
{
    [SerializeField] private GameObject blade;
    public GameObject OwningPlayer;
    public int damage = 10;
    private float originalScale = 0;
    public bool canAttack = true;

    [Header("Sounds")]
    [SerializeField] private AudioClip hitAudio;

    private void Awake()
    {
        originalScale = blade.transform.localScale.x;
    }

    private void Update()
    {
        blade.transform.Rotate(0, 0, 500f *  Time.deltaTime);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != OwningPlayer)
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                if (canAttack)
                {
                    PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
                    hittedPlr.ChangeHealth(hittedPlr.stats.health - damage);
                    UIScript.Instance.OnHit(hittedPlr);
                    UIScript.Instance.UpdateHealth(hittedPlr);
                    StartCoroutine(OnAttacked());
                }
                
                Vector2 bounceDirection = (collision.transform.position - transform.position).normalized;
                playerRb.velocity = Vector2.zero;
                playerRb.AddForce(bounceDirection * 12.0f, ForceMode2D.Impulse);

                AudioManager.Instance.PlayAudio(hitAudio);

                blade.transform.localScale = new Vector3(originalScale * 2, originalScale * 2, originalScale * 2);
                blade.transform.DOScale(new Vector3(originalScale, originalScale, originalScale), .5f);
            }
        }
    }

    IEnumerator OnAttacked()
    {
        canAttack = false;

        yield return new WaitForSeconds(2f);

        canAttack = true;

        yield return null;
    }
}
