using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : MonoBehaviour
{
    public float speed = 3f;
    public float lifetime = 5f;
    public float vineUpTime;
    public float vineTime;
    public bool reachedLimit = false;
    public bool vineFinished = false;

    private void Start()
    {
        //Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        vineUpTime = 2;
        vineTime = Time.deltaTime;

        if (reachedLimit == false)
        {
            StartCoroutine(VineUP());
        }

        //if (reachedLimit == true)
        //{
        //    StartCoroutine(VineDown());
        //}

        if (vineFinished == true)
        {
            Destroy(gameObject, 1);
        }
    }

    IEnumerator VineUP()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
        yield return new WaitForSeconds (vineUpTime);
        reachedLimit = true;
        StartCoroutine(VineDown());
    }

    IEnumerator VineDown()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
        yield return new WaitForSeconds(vineUpTime);
        vineFinished = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<MoveFSM>().TakeDamage();
        }
    }
}
