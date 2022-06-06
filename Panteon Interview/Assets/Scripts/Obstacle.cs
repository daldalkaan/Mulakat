using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum ObstaclesType
    {
        Rotate,
        Move
    }

    public ObstaclesType obstaclesType;
    [Header("Rotate")]
    public Vector3 deltaRot;
    [Header("Move")]
    private Vector3 defaultPosition;
    private bool isForward = true;
    public float speed, distance = 5f;
    void Awake()
    {
        switch (obstaclesType)
        {
            case ObstaclesType.Rotate:
                break;
            case ObstaclesType.Move:
                defaultPosition = transform.position;
                transform.position += Vector3.forward * Random.Range(0, (int)distance);
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        switch (obstaclesType)
        {
            case ObstaclesType.Rotate:
                transform.Rotate(deltaRot, Space.Self);
                break;
            case ObstaclesType.Move:
                if (isForward)
                {
                    if (transform.position.z < defaultPosition.z + distance)
                    {
                        transform.position += Vector3.forward * Time.deltaTime * speed;
                    }
                    else
                        isForward = false;
                }
                else
                {
                    if (transform.position.z > defaultPosition.z)
                    {
                        transform.position -= Vector3.forward * Time.deltaTime * speed;
                    }
                    else
                        isForward = true;
                }
                break;
            default:
                break;
        }
    }
}
