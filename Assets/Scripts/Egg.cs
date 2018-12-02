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
    [SerializeField] private Vector2 jumpForce;
    [SerializeField] private float jumpChance;
    [SerializeField] private float jumpCheckTime;
    [SerializeField] private float jumpCheckTimeFudge;
    [SerializeField] private LayerMask terrainLayerMask;
    private float nextJumpCheckTime;

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
        SetNextJumpCheckTime();
    }
	
	// Update is called once per frame
	void Update () {
        DoAnimation();
		if (isGrounded && !isCarried)
        {
            if (Time.time > nextJumpCheckTime)
            {
                SetNextJumpCheckTime();
                if (Random.value < jumpChance)
                {
                    rigidbody2D.AddForce(new Vector2(jumpForce.x * direction, jumpForce.y), ForceMode2D.Impulse);
                    isGrounded = false;
                }
            }
            else
            {
                RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, new Vector2(direction, -1), 0.5f, terrainLayerMask);
                if (!raycastHit.transform)
                    direction *= -1;
                float moveHoriz = direction * movementFactor;
                rigidbody2D.velocity = new Vector2(moveHoriz, rigidbody2D.velocity.y);
                transform.localScale = new Vector3(direction, 1, 1);
                animState = AnimState.WALKING;
            }
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
            turnPoint.enabled = false;
        }
        else if (rigidbody2D.velocity.y < -0.1f)
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

    void SetNextJumpCheckTime()
    {
        nextJumpCheckTime = Time.time + jumpCheckTime;
        nextJumpCheckTime += Random.Range(-jumpCheckTimeFudge, jumpCheckTimeFudge);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Egg Collided: " + collision.collider + collision.otherCollider);
        if (collision.otherCollider == footCollider)
        {
            isGrounded = true;
            turnPoint.enabled = true;
            animState = AnimState.WALKING;
        } else if (collision.otherCollider.tag == "Enemy")
        {
            direction *= -1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        direction *= -1;
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
        turnPoint.enabled = true;
        rigidbody2D.simulated = true;
        rigidbody2D.velocity = Vector2.zero; // zero out velocity before throwing
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
