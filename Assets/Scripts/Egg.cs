using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : MonoBehaviour {

    [Header("Sprite Properties")]
    [SerializeField] private AnimState animState;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite carriedSprite;
    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private Sprite shockedSprite;
    [SerializeField] private float walkSpriteTime;
    [SerializeField] private float crackedFlashTime;
    [SerializeField] private float crackedFlashCount = 10;
    [SerializeField] private int direction = 1;

    private int currentAnimSprite;
    private float nextAnimSpriteTime;
    private int crackedFlash = 0;

    private SpriteRenderer spriteRenderer;
    [Header("Movement Properties")]
    [SerializeField] private float movementFactor;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isCarried = false;
    [SerializeField] private bool isCracked = false;
    [SerializeField] private bool isShocked = false;
    [SerializeField] private Vector2 jumpForce;
    [SerializeField] private float jumpChance;
    [SerializeField] private LayerMask terrainLayerMask;
    private float nextJumpCheckTime;

    [Header("Balance Properites")]
    [SerializeField] private float jumpCheckTime;
    [SerializeField] private float jumpCheckTimeFudge;
    [SerializeField] private float shockLookDistance;
    [SerializeField] private float shockedTime;

    private new Rigidbody2D rigidbody2D;
    private Collider2D footCollider;
    private Collider2D turnPoint;
    private Transform lookOrigin;
    private GameController controller;

    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        footCollider = transform.Find("FootCollider").GetComponent<Collider2D>();
        turnPoint = transform.Find("TurnPoint").GetComponent<Collider2D>();
        lookOrigin = transform.Find("LookOrigin");
        controller = FindObjectOfType<GameController>();
        SetNextJumpCheckTime();
    }
	
	// Update is called once per frame
	void Update () {
        DoAnimation();
		if (isGrounded && !isCarried)
        {
            if (isShocked)
            {
                if (Time.time > nextAnimSpriteTime)
                    isShocked = false;
            }
            else
            {
                Look();
                if (Time.time > nextJumpCheckTime)
                {
                    CheckJump();
                }
                else
                {
                    Walk();
                }
            }
        } else
        {
            animState = AnimState.IDLE;
        }
	}

    private void Look()
    {
        Debug.DrawLine(lookOrigin.position, lookOrigin.position + new Vector3(direction * shockLookDistance, 0, 0));
        RaycastHit2D raycastHit = Physics2D.Raycast(lookOrigin.position,
                                                    new Vector2(direction * shockLookDistance, 0),
                                                    shockLookDistance);
        if (!raycastHit.transform)
            return;

        if (raycastHit.transform.tag == "Enemy")
        {
            var hitObject = raycastHit.transform.GetComponent<Egg>();
            if (hitObject)
            {
                if (hitObject.isCarried)
                {
                    GetShocked();
                    return;
                }
            }
        } else if (raycastHit.transform.tag == "Player")
        {
            var hitObject = raycastHit.transform.GetComponent<PlayerController>();
            if (hitObject)
            {
                if (hitObject.IsCarrying)
                {
                    GetShocked();
                    return;
                }
            }
        }
    }

    private void GetShocked()
    {
        controller.RegisterShock();
        isShocked = true;
        nextAnimSpriteTime = Time.time + shockedTime;
    }

    private void Walk()
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, new Vector2(direction, -1), 0.5f, terrainLayerMask);
        if (!raycastHit.transform)
            direction *= -1;
        float moveHoriz = direction * movementFactor;
        rigidbody2D.velocity = new Vector2(moveHoriz, rigidbody2D.velocity.y);
        transform.localScale = new Vector3(direction, 1, 1);
        animState = AnimState.WALKING;
    }

    private void CheckJump()
    {
        SetNextJumpCheckTime();
        if (Random.value < jumpChance)
        {
            rigidbody2D.AddForce(new Vector2(jumpForce.x * direction, jumpForce.y), ForceMode2D.Impulse);
            isGrounded = false;
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
        if (isShocked)
        {
            spriteRenderer.sprite = shockedSprite;
        } else if (isCarried)
        {
            spriteRenderer.sprite = carriedSprite;
        } else if (isCracked) {
            spriteRenderer.sprite = crackedSprite;
            if (Time.time > nextAnimSpriteTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                nextAnimSpriteTime = Time.time + crackedFlashTime;
                crackedFlash++;
            }
            if (crackedFlash >= crackedFlashCount)
            {
                Destroy(gameObject);
            }
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

    void DoCrack()
    {
        rigidbody2D.simulated = false;
        isCracked = true;
        isCarried = false;
        nextAnimSpriteTime = Time.time + crackedFlashTime;
        controller.EggDied();
    }

    void SetNextJumpCheckTime()
    {
        nextJumpCheckTime = Time.time + jumpCheckTime;
        nextJumpCheckTime += Random.Range(-jumpCheckTimeFudge, jumpCheckTimeFudge);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Finish")
        {
            DoCrack();
        } else if (collision.otherCollider == footCollider)
        {
            isGrounded = true;
            turnPoint.enabled = true;
            animState = AnimState.WALKING;
        } else if (collision.collider.tag == "Enemy")
        {
            direction *= -1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            controller.EggAppeased(gameObject, collision.gameObject);
            Destroy(collision.gameObject);
        } else
        {
            direction *= -1;
        }
    }

    public void SetRandomDirection()
    {
        direction = 1;
        float choice = Random.value;
        if (choice < 0.5f)
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
