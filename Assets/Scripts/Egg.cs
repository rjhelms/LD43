using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour {

    [Header("Sprite Properties")]
    [SerializeField] private AnimState animState;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite carriedSprite;
    [SerializeField] private float walkSpriteTime;
    [SerializeField] private int direction = 1;
    private int currentAnimSprite;
    private float nextAnimSpriteTime;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Properties")]
    [SerializeField] private float movementFactor;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isCarried = false;
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
        // don't collide with terrain when moving up
        if (rigidbody2D.velocity.y > 0)
        {
            footCollider.enabled = false;
        }
        else if (rigidbody2D.velocity.y < -0.1)
        {
            footCollider.enabled = true;
        }
    }

    void DoAnimation()
    {
        if (isCarried)
        {
            spriteRenderer.sprite = carriedSprite;
        }
        else
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Egg Collided: " + collision.collider + collision.otherCollider);
        if (collision.otherCollider == footCollider)
        {
            isGrounded = true;
            animState = AnimState.WALKING;
        } else if (collision.otherCollider == turnPoint)
        {
            direction *= -1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "TurnPoint" && isGrounded)
            {
            direction *= -1;
        }
    }

    public void Carry()
    {
        isCarried = true;
        rigidbody2D.simulated = false;
    }

    public void Throw(Vector2 throwForce)
    {
        isCarried = false;
        isGrounded = false;
        rigidbody2D.simulated = true;
        rigidbody2D.AddForce(throwForce, ForceMode2D.Impulse);
        if (throwForce.x < 0)
        {
            direction = -1;
        } else
        {
            direction = 1;
        }
    }
}
