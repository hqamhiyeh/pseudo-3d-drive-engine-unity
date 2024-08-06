using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float _horizontal;
    private float _speed = 8f;

    [SerializeField] private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate() {
        rb.velocity = new Vector2(_horizontal * _speed, rb.velocity.y);
    }
}
