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
    [Header("")]
    public Vector3 movementVector;
    public float period = 4f;
    Vector3 startingpos;
    float movementFactor;
    void Awake()
    {
        switch (obstaclesType)
        {
            case ObstaclesType.Move:
                period += Random.Range(0, 0.5f);
                startingpos = transform.position;
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (obstaclesType)
        {
            case ObstaclesType.Rotate:
                Rotate();
                break;
            case ObstaclesType.Move:
                Move();
                break;
        }
    }

    public void Move()
    {
        if (period <= 0f) { return; }
        float cycles = Time.time / period;

        const float tau = Mathf.PI * 2;
        float rawSineWave = Mathf.Sin(cycles * tau);

        movementFactor = rawSineWave / 2f + 0.5f;

        Vector3 offset = movementVector * movementFactor;
        transform.position = startingpos + offset;
    }
    public void Rotate() => transform.Rotate(movementVector, Space.Self);
}
