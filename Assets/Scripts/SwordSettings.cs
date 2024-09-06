using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSettings : MonoBehaviour
{
    public SwordTypes type;
    public float coolTime;
    public int attackDamage;
    public PolygonCollider2D collideRange;

    public int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
}
