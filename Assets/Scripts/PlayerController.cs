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
    [SerializeField] private Vector2 throwForce;
    private float nextLiftTime;

    private new Rigidbody2D rigidbody2D;
    private Collider2D footCollider;
    private Transform pickupPoint;
    private Transform carryPoint;
    private Transform carriedObject;
    private GameController controller;
    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        footCollider = transform.Find("FootCollider").GetComponent<Collider2D>();
        pickupPoint = transform.Find("PickupPoint");
        carryPoint = transform.Find("CarryPoint");
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
            if (Input.GetButtonDown("Fire2") && isGrounded)
            {
                isGrounded = false;
                rigidbody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            }
            if (Input.GetButtonDown("Fire1"))
            {
                if (isCarrying)
                {
                    DoThrow();
                }
                else
                {
                    DoLift();
                }
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
                    if (isCarrying)
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
                        if (isCarrying)
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

    void DoLift()
    {
        isLifting = true;
        nextLiftTime = Time.time + liftTimeOut;
        RaycastHit2D raycastHit = Physics2D.Raycast(pickupPoint.position, new Vector2(transform.localScale.x, 0),
                                                    liftRayCastDistance, liftRayCastMask);
        if (raycastHit.transform)
        {
            carriedObject = raycastHit.transform;
            Debug.Log("Lifting " + raycastHit.transform);
            carriedObject.SetParent(carryPoint);
            carriedObject.localPosition = Vector3.zero; // at the carry point
            carriedObject.localScale = Vector3.one;
            isCarrying = true;
            isLifting = false;
            carriedObject.GetComponent<Egg>().Carry(); // assuming only eggs can be carried.
        }
    }

    void DoThrow()
    {
        isCarrying = false;
        carriedObject.SetParent(null);
        carriedObject.GetComponent<Egg>().Throw(throwForce * transform.localScale);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider == footCollider)
        {
            Debug.Log("Landed!");
            isGrounded = true;
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