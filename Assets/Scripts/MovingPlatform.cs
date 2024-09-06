using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType { movingBlockY, movingBlockX, Saw, SawX, SawY }

public class MovingPlatform : MonoBehaviour
{
    public bool isMove = false;
    public float floatHeight = 1.0f;
    public float floatSpeed = 1.0f;

    public ObstacleType type;


    private void Update()
    {
        if (isMove)
        {
            Move();
        }
    }

    private void Move()
    {
        // Sin 함수를 사용하여 오브젝트를 위아래로 움직임
        if (type == ObstacleType.movingBlockX || type == ObstacleType.SawX)
        {
            float newX = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
        else if (type == ObstacleType.movingBlockY || type == ObstacleType.SawY)
        {
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

}
