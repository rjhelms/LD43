using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {

    public bool Active = true;

    private new Rigidbody2D rigidbody2D;
    private GameController controller;
    private float timeOut;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        controller = FindObjectOfType<GameController>();
        timeOut = Time.time + controller.CoinTimeout;
    }

    private void Update()
    {
        if (Time.time > timeOut)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Terrain")
        {
            if (rigidbody2D.velocity.y < 0)
            {
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.isKinematic = true;
            }
        }
    }
}
