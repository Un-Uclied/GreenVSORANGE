using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI.Table;

public struct SwordStats
{
    public bool onCoolTime;
    public float coolTime;
    public int attackDamage;

    public int[] triangles;
}

public class SwordScript : MonoBehaviour
{
    public PlayerMovement plrMovement;
    [SerializeField] private GameObject spriteObject;
    [SerializeField] private GameObject attackRangeObject;
    public SwordSettings module;

    public SwordTypes swordType;
    public SwordStats stats;

    private void Awake()
    {
        RoundManager.Instance.OnRoundStarted += OnRoundStarted;
        plrMovement.plrDied += OnPlayerDied;
       
    }

    public void PlayerAttack(/*InputAction.CallbackContext context*/)
    {
        //if (!context.started) { return; }
        if (stats.onCoolTime) { return; }
        if (plrMovement.stats.isAnchored) { return; }

        StartCoroutine(SwingSword());
        StartCoroutine(ApplyCooltime());
        StartCoroutine(attackRangeObject.GetComponent<SwordColliderScript>().OnAttack());
        AttackHandler.Instance.Attack(plrMovement, this);
    }

    public void SwordPosition()
    {
        if (plrMovement.stats.isAnchored) { return; }
        
        Vector2 target = Vector2.zero;
        float targetRot = 0f;

        switch (plrMovement.stats.watchingDir)
        {
            case WatchingDirection.Left:
                target = new Vector2(.2f, -.25f);
                targetRot = 0f;
                break;
            case WatchingDirection.LeftDown:
                target = new Vector2(0.64f, 0.024f);
                targetRot = 51f;
                break;
            case WatchingDirection.LeftUp:
                target = new Vector2(0.19f, -0.39f);
                targetRot = -33f;
                break;
            case WatchingDirection.Right:
                target = new Vector2(-.2f, -.25f);
                targetRot = 0f;
                break;
            case WatchingDirection.RightUp:
                target = new Vector2(-0.19f, -0.39f);
                targetRot = -120;
                break;
            case WatchingDirection.RightDown:
                target = new Vector2(-0.082f, 0.36f);
                targetRot = 114;
                break;
            case WatchingDirection.FlipedUp:
                target = new Vector2(0f, -.25f);
                targetRot = -90;
                break;
            case WatchingDirection.FlipedDown:
                target = new Vector2(0f, .25f);
                targetRot = 90f;
                break;
            case WatchingDirection.Up:
                target = new Vector2(0f, -.25f);
                targetRot = -90f;
                break;
            case WatchingDirection.Down:
                target = new Vector2(0f, .25f);
                targetRot = 90f;
                break;
        }

        spriteObject.transform.DOLocalMove(target, .4f);
        spriteObject.transform.DOLocalRotate(new Vector3(0, 0, targetRot), .5f);
    }
   
    private IEnumerator SwingSword()
    {
        SwordPosition();
        spriteObject.transform.DOComplete();

        float targetRot = 0f;
        float originalRot = spriteObject.transform.eulerAngles.z;
        switch (plrMovement.stats.watchingDir)
        {
            case WatchingDirection.Left:
                targetRot = 160f;
                break;
            case WatchingDirection.LeftDown:
                targetRot = 180f;
                break;
            case WatchingDirection.LeftUp:
                targetRot = 100f;
                break;
            case WatchingDirection.Right:
                targetRot = -160f;
                break;
            case WatchingDirection.RightUp:
                targetRot = -0;
                break;
            case WatchingDirection.RightDown:
                targetRot = 260;
                break;
            case WatchingDirection.FlipedUp:
                targetRot = 50;
                break;
            case WatchingDirection.FlipedDown:
                targetRot = 220;
                break;
            case WatchingDirection.Up:
                targetRot = 50;
                break;
            case WatchingDirection.Down:
                targetRot = 220;
                break;
        }

        spriteObject.transform.DOLocalRotate(new Vector3(0, 0, targetRot), .1f);
        yield return new WaitForSeconds(.05f);
        spriteObject.transform.DOLocalRotate(new Vector3(0, 0, originalRot), .3f);

        yield return null;
    }

    public void ApplySwordChanges(SwordSettings changeSword)
    {
        module = changeSword;
        stats.coolTime = changeSword.coolTime;
        stats.attackDamage = changeSword.attackDamage;
        
        PolygonCollider2D col = attackRangeObject.GetComponent<PolygonCollider2D>();
        col.points = changeSword.collideRange.points;

        swordType = changeSword.type;

        GameObject clonedSword = Instantiate(changeSword.transform.Find("Sprite")).gameObject;
        Destroy(spriteObject);
        clonedSword.transform.SetParent(transform);
        spriteObject = clonedSword;

        stats.triangles = changeSword.triangles;
        DrawCollider();
    }

    private void DrawCollider()
    {
        MeshFilter meshFilter = attackRangeObject.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.Clear();

        PolygonCollider2D polygon = attackRangeObject.GetComponent<PolygonCollider2D>();

        Vector3[] verticies = new Vector3[polygon.points.Length];
        for (int i = 0; i < polygon.points.Length; i++)
        {
            verticies[i] = new Vector3(polygon.points[i].x, polygon.points[i].y, 0);
        }

        mesh.vertices = verticies;
        mesh.triangles = stats.triangles;

        meshFilter.mesh = mesh;
    }

    private IEnumerator ApplyCooltime()
    {
        stats.onCoolTime = true;
        yield return new WaitForSeconds(stats.coolTime);
        stats.onCoolTime = false;
        yield return null;
    }

    private void OnPlayerDied(PlayerMovement plr)
    {
        attackRangeObject.GetComponent<MeshRenderer>().enabled = false;
        spriteObject.SetActive(false);
    }

    private void OnRoundStarted()
    {
        spriteObject.SetActive(true);
        spriteObject.transform.localPosition = new Vector3(0.3f, -0.2f, 0);
        spriteObject.transform.rotation = Quaternion.identity;
        attackRangeObject.GetComponent<MeshRenderer>().enabled = true;
        stats.attackDamage = module.attackDamage + plrMovement.extraStats.extraAttackDamage;
        stats.coolTime = module.coolTime - plrMovement.extraStats.extraAttackSpeed;
    }

    public void RotateCollider()
    {
        if (plrMovement.stats.isAnchored) { return; }
        switch (plrMovement.stats.watchingDir)
        {
            case WatchingDirection.Left:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case WatchingDirection.LeftDown:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 45);
                break;
            case WatchingDirection.LeftUp:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, -45);
                break;
            case WatchingDirection.Right:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case WatchingDirection.RightUp:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, -135);
                break;
            case WatchingDirection.RightDown:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 135);
                break;
            case WatchingDirection.FlipedUp:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case WatchingDirection.FlipedDown:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case WatchingDirection.Up:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case WatchingDirection.Down:
                attackRangeObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
    }
}
