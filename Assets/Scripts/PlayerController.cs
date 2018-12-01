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
    [SerializeField] private float walkSpriteTime;

    private int currentAnimSprite;
    private float nextAnimSpriteTime;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Properties")]
    [SerializeField] private float movementFactor;
    [SerializeField] private float movementDeadZone;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool isGrounded = true;

    private new Rigidbody2D rigidbody2D;
    private Collider2D footCollider;
    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        footCollider = transform.Find("FootCollider").GetComponent<Collider2D>();
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
        if (Input.GetButtonDown("Fire1") && isGrounded ) {
            isGrounded = false;
            rigidbody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
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
        }
    }
}