using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordColliderScript : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private SwordScript sword;

    [Header("Materials")]
    [SerializeField] private Material onAttackMat;
    [SerializeField] private Material offAttackMat;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") && collision.transform.gameObject != player)
        {
            if (collision.transform.GetComponent<PlayerMovement>().stats.isInvincible) { return; }
            if (sword.module.attackDamage == 0) { return; }
            collision.transform.gameObject.GetComponent<PlayerMovement>().ChangeHealth(collision.transform.gameObject.GetComponent<PlayerMovement>().stats.health - sword.stats.attackDamage);

            AttackHandler.Instance.OnHitted(player, collision.transform.gameObject, sword);
        }
    }

    public IEnumerator OnAttack()
    {
        GetComponent<PolygonCollider2D>().enabled = true;
        GetComponent<MeshRenderer>().material = onAttackMat;

        yield return new WaitForSeconds(.05f);

        GetComponent<PolygonCollider2D>().enabled = false;
        GetComponent<MeshRenderer>().material = offAttackMat;
        yield return null;
    }
}
