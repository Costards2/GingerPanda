using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FireBall : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody2D rb;
    public string target;
    
 
    void Start()
    {
        //rb.velocity = transform.forward * speed; 
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        StartCoroutine(destroyArrow());
    }
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("" + target))
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }

    IEnumerator destroyArrow()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);

    }
}
