using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3[] targets = new Vector3[]
    {
        new Vector3(-80.0f, 9.0f, -30.2f),
        new Vector3(-63.0f, 9.0f, -30.2f),
        new Vector3(-40.5f, 7.5f, -30.2f),
        new Vector3(-22.0f, 9.0f, -30.2f),
        new Vector3(-5.0f, 9.0f, -30.2f)
    };
    public float speed = 2.0f;
    private int currentTarget = 0;

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targets[currentTarget], speed * Time.deltaTime);

        if (transform.position == targets[currentTarget])
        {
            currentTarget = (currentTarget + 1) % targets.Length;
        }
    }
}