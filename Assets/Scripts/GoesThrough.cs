using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoesThrough : MonoBehaviour
{
    private Collider2D _collider;
    private bool _playerOnPlataform;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerOnPlataform && Input.GetAxisRaw("Vertical") < 0 )
        {
            _collider.enabled = false;
            StartCoroutine(EnableCollider());
        }
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.5f);
        _collider.enabled = true;
    }

    private void SetPlaterOnPlataform(Collision2D other , bool value)
    {
        var player = other.gameObject.GetComponent<MoveFSM>();

        if (player != null)
        {
            _playerOnPlataform = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        SetPlaterOnPlataform(other, true);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        SetPlaterOnPlataform(other, true);
    }
}
