using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class FireBall : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody2D rb;
    public string target;
    GameObject player;
    float facing;
  
    void Start()
    {
        //Mal otimizado para porra 
        player = GameObject.FindWithTag("Player");
        facing = player.GetComponent<MoveFSM>().facing;
        rb.velocity = new Vector2(speed * - facing , 0);

        StartCoroutine(destroyArrow());
    }

    void Update()
    {
        //transform.Translate(Vector2.right * speed * Time.deltaTime);
        //StartCoroutine(destroyArrow());
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
