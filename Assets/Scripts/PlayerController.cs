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
    [SerializeField] private Vector2 movementFactor;
    [SerializeField] private float movementDeadZone;

    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        DoAnimation();
        DoInput();
    }

    void DoInput()
    {
        Vector2 moveVector = new Vector2(0, 0);
        float rawInputHoriz = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(rawInputHoriz) > movementDeadZone)
        {
            moveVector += new Vector2(rawInputHoriz, 0);
            moveVector *= movementFactor;
            GetComponent<Rigidbody2D>().velocity = moveVector;
            animState = AnimState.WALKING;
            if (rawInputHoriz < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            } else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        } else {
            animState = AnimState.IDLE;
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
}