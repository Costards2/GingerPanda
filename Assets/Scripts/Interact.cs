using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private Transform InteractableCheck;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private SpriteRenderer sprite;
    public bool interacting; 

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //Debug.Log(sprite.color);
        //Debug.Log(CanInteract());

        if (CanInteract())
        {
            interacting = true;
            sprite.color = Color.red;
        }
        else
        {
            interacting = false;
            sprite.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private bool CanInteract()
    {
        return Physics2D.OverlapCircle(InteractableCheck.position, 2f, playerLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(InteractableCheck.position, 2f);
    }
}
