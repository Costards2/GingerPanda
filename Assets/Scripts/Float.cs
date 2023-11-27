using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    public float minHeight = -0.1f;
    public float maxHeight = 0.3f;
    public float floatSpeed = 0.2f;
    private float initialY;


    void Start()
    {
        initialY = transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        float newY = Mathf.PingPong(Time.time * floatSpeed, maxHeight - minHeight) + minHeight;
        transform.position = new Vector2(transform.position.x, newY + initialY);

    }
}
