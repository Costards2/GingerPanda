using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject player;
    public Rigidbody2D rb;
    public string target;
    public float force = 10;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        Vector3 direction = player.transform.position - transform.position;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;

        float rot = Mathf.Atan2(-direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);

        StartCoroutine(destroyArrow());
    }

    void Update()
    {
        
    }

    //void OnTriggerEnter2D(Collider2D hitInfo)
    //{
    //    if (hitInfo.CompareTag("" + target))
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

    IEnumerator destroyArrow()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);

    }
}
