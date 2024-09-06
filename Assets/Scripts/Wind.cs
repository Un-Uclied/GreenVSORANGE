using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public GameObject ownedPlayer;
    public int damage;

    private void Start()
    {
        float distanceOrange = Vector3.Distance(transform.position, ItemManager.Instance.playerOrange.transform.position);
        float distanceGreen = Vector3.Distance(transform.position, ItemManager.Instance.playerGreen.transform.position);

        ownedPlayer = (distanceOrange < distanceGreen) ? ItemManager.Instance.playerOrange : ItemManager.Instance.playerGreen;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject != ownedPlayer)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }
            PlayerMovement hittedPlr = collision.transform.GetComponent<PlayerMovement>();
            hittedPlr.ChangeHealth(hittedPlr.stats.health - damage);
            UIScript.Instance.OnHit(hittedPlr);
            UIScript.Instance.UpdateHealth(hittedPlr);
        }
    }
}
