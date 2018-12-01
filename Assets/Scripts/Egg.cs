using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour {

    [Header("Sprite Properties")]
    [SerializeField] private AnimState animState;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private float walkSpriteTime;
    [SerializeField] private int direction = 1;
    private int currentAnimSprite;
    private float nextAnimSpriteTime;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Properties")]
    [SerializeField] private float movementFactor;
    [SerializeField] private bool isGrounded = true;

    private new Rigidbody2D rigidbody2D;
    private Collider2D footCollider;
    private Collider2D turnPoint;
    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        footCollider = transform.Find("FootCollider").GetComponent<Collider2D>();
        turnPoint = transform.Find("TurnPoint").GetComponent<Collider2D>();
    }
	
	// Update is called once per frame
	void Update () {
        DoAnimation();
		if (isGrounded)
        {
            float moveHoriz = direction * movementFactor;
            rigidbody2D.velocity = new Vector2(moveHoriz, rigidbody2D.velocity.y);
            transform.localScale = new Vector3(direction, 1, 1);
            animState = AnimState.WALKING;
        } else
        {
            animState = AnimState.IDLE;
        }
	}

    private void FixedUpdate()
    {
        // don't collide with terrain when jumping up
        if (rigidbody2D.velocity.y > 0)
        {
            footCollider.enabled = false;
            isGrounded = false;
        }
        else
        {
            if (rigidbody2D.velocity.y < -0.1)
                isGrounded = false;
            footCollider.enabled = true;
        }
    }

    void DoAnimation()
    {
        switch (animState)
        {
            case AnimState.IDLE:
                spriteRenderer.sprite = idleSprite;
                nextAnimSpriteTime = Time.time;
                break;
            case AnimState.WALKING:
                if (Time.time > nextAnimSpriteTime)
                {
                    currentAnimSprite++;
                    currentAnimSprite %= walkSprites.Length;
                    spriteRenderer.sprite = walkSprites[currentAnimSprite];
                    nextAnimSpriteTime += walkSpriteTime;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider == footCollider)
        {
            Debug.Log("Landed!");
            isGrounded = true;
            animState = AnimState.WALKING;
        } else if (collision.otherCollider == turnPoint)
        {
            direction *= -1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TurnPoint")
            {
            direction *= -1;
        }
    }
}
