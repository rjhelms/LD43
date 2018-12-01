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

    // Use this for initialization
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        DoAnimation();
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