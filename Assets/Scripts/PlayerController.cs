using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum AnimState
{
    IDLE,
    WALKING,
}
public class PlayerController : MonoBehaviour
{

    [Header("Sprite Properties")]
    [SerializeField] private AnimState animState;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] carryWalkSprites;
    [SerializeField] private Sprite carryIdleSprite;
    [SerializeField] private Sprite liftSprite;
    [SerializeField] private float walkSpriteTime;

    private int currentAnimSprite;
    private float nextAnimSpriteTime;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Properties")]
    [SerializeField] private float movementFactor;
    [SerializeField] private float movementDeadZone;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isLifting = false;
    [SerializeField] private bool isCarrying = false;
    [SerializeField] private float liftTimeOut;
    [SerializeField] private float liftRayCastDistance;
    [SerializeField] private LayerMask liftRayCastMask;
    [SerializeField] private Vector2 throwEggForce;
    [SerializeField] private Vector2 throwCoinForce;

    private float nextLiftTime;
    private int jumps;

    [Header("Coin Properties")]
    [SerializeField] private GameObject coinPrefab;
    
    private new Rigidbody2D rigidbody2D;
    private Collider2D footCollider;
    private Transform pickupPoint;
    private Transform carryPoint;
    private Transform carriedObject;
    private Transform coinParent;
    private GameController controller;

    public bool IsCarrying
    {
        get
        {
            return isCarrying;
        }

        private set
        {
            isCarrying = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        footCollider = transform.Find("FootCollider").GetComponent<Collider2D>();
        pickupPoint = transform.Find("PickupPoint");
        carryPoint = transform.Find("CarryPoint");
        coinParent = GameObject.Find("Coins").transform;
        controller = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        DoAnimation();
        DoInput();
    }

    private void FixedUpdate()
    {
        // don't collide with terrain when jumping up
        if (rigidbody2D.velocity.y > 0)
        {
            footCollider.enabled = false;
        } else
        {
            footCollider.enabled = true;
        }
    }

    void DoInput()
    {
        float rawInputHoriz = Input.GetAxisRaw("Horizontal");
        float moveHoriz = 0;
        if (Mathf.Abs(rawInputHoriz) > movementDeadZone)
        {
            if (rawInputHoriz < 0)
            {
                moveHoriz = -1;
                transform.localScale = new Vector3(-1, 1, 1);
            } else
            {
                moveHoriz = 1;
                transform.localScale = new Vector3(1, 1, 1);
            }
            moveHoriz *= movementFactor;
            animState = AnimState.WALKING;
        } else {
            moveHoriz = 0;
            animState = AnimState.IDLE;
        }
        rigidbody2D.velocity = new Vector2(moveHoriz, rigidbody2D.velocity.y);
        if (!isLifting)
        {
            if (Input.GetButtonDown("Fire2") && jumps < 2)
            {
                isGrounded = false;
                jumps++;
                rigidbody2D.AddForce(new Vector2(0, jumpForce / Mathf.Sqrt(jumps)), ForceMode2D.Impulse);
            }
            if (Input.GetButtonDown("Fire1"))
            {
                if (IsCarrying)
                {
                    ThrowEgg();
                }
                else
                {
                    LiftEgg();
                }
            }
            if (Input.GetButtonDown("Fire3") && !IsCarrying)
            {
                ThrowCoin();
            }
        }
    }

    void DoAnimation()
    {
        if (isLifting)
        {
            spriteRenderer.sprite = liftSprite;
            if (Time.time > nextLiftTime)
            {
                isLifting = false;
            }
        } else {
            switch (animState)
            {
                case AnimState.IDLE:
                    if (IsCarrying)
                    {
                        spriteRenderer.sprite = carryIdleSprite;
                    }
                    else
                    {
                        spriteRenderer.sprite = idleSprite;
                    }
                    nextAnimSpriteTime = Time.time;
                    break;
                case AnimState.WALKING:
                    if (Time.time > nextAnimSpriteTime)
                    {
                        currentAnimSprite++;
                        currentAnimSprite %= walkSprites.Length;
                        if (IsCarrying)
                        {
                            spriteRenderer.sprite = carryWalkSprites[currentAnimSprite];
                        }
                        else
                        {
                            spriteRenderer.sprite = walkSprites[currentAnimSprite];
                        }
                        nextAnimSpriteTime += walkSpriteTime;
                    }
                    break;
            }
        }
    }

    void LiftEgg()
    {
        isLifting = true;
        nextLiftTime = Time.time + liftTimeOut;
        RaycastHit2D raycastHit = Physics2D.Raycast(pickupPoint.position, new Vector2(transform.localScale.x, 0),
                                                    liftRayCastDistance, liftRayCastMask);
        if (raycastHit.transform)
        {
            carriedObject = raycastHit.transform;
            carriedObject.SetParent(carryPoint);
            carriedObject.localPosition = Vector3.zero; // at the carry point
            carriedObject.localScale = Vector3.one;
            IsCarrying = true;
            isLifting = false;
            carriedObject.GetComponent<Egg>().Carry(); // assuming only eggs can be carried.
        }
    }

    void ThrowEgg()
    {
        IsCarrying = false;
        carriedObject.SetParent(controller.EggParent);
        carriedObject.GetComponent<Egg>().Throw(throwEggForce * transform.localScale);
    }

    void ThrowCoin()
    {
        if (controller.TryThrowCoin())
        {
            GameObject coin = Instantiate(coinPrefab, carryPoint.position, Quaternion.identity, coinParent);
            coin.GetComponent<Rigidbody2D>().AddForce(throwCoinForce * transform.localScale, ForceMode2D.Impulse);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider == footCollider)
        {
            isGrounded = true;
            jumps = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PowerUp")
        {
            controller.GetMoney(collision.gameObject);
        }
    }
}