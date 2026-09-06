using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerControl : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] float moveSpeed;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;

    [Header("Recoil Settings")]
    [SerializeField] float recoilTime;
    [SerializeField] float recoilSpeed;

    [Header("Health/Damage Settings")]
    [SerializeField] int maxHealth;
    [SerializeField] float invincibleTime;
    [SerializeField] float flashInterval;

    // Player components
    Rigidbody2D rb;
    SpriteRenderer sr;

    // Private variables
    Vector2 moveInput;
    Vector2 lastMoveDirection;
    bool canDash = true;
    bool dashPressed;

    // Player state
    bool isDashing;
    bool isRecoiling;
    bool isInvincible;

    int curHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lastMoveDirection = new Vector2(0, 1);
    }

    void Update()
    {
        ReadInput();
        Move();
        HandleDash();
    }

    void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        moveInput.x = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput.x += 1f;

        moveInput.y = 0f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput.y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput.y += 1f;

        moveInput.Normalize();

        dashPressed = false;
        if (keyboard.spaceKey.isPressed) dashPressed = true;
        
    }

    /* MOVEMENTS */

    // Control player's basic movement
    void Move()
    {
        if (moveInput.magnitude != 0) lastMoveDirection = moveInput;
        if (!isDashing)
            rb.linearVelocity = new Vector2(moveSpeed * moveInput.x, moveSpeed * moveInput.y);
    }

    // Control player's dash ability
    void HandleDash()
    {
        if (dashPressed && canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    // The heavy lifting part of dash
    IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        Vector2 dashDir = moveInput;
        if (dashDir == Vector2.zero) dashDir = lastMoveDirection;

        // Increase velocity to dash mode
        rb.linearVelocity = dashDir * dashSpeed; 

        float timer = 0f;
        while (timer < dashTime)
        {
            // Can change direction mid-dash (i.e. direction not locked)
            if (moveInput.magnitude != 0)
                rb.linearVelocity = moveInput * dashSpeed;

            timer += Time.deltaTime;
            yield return null;
        }
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    /* HEALTH & DAMAGE */

    // Used by hazards (e.g. bullets) to deal damage
    public void TakeDamage(Vector2 hitPosition, int increment)
    {
        curHealth -= increment;
        TriggerRecoil(hitPosition);
        BeginInvincibity();

        if (curHealth <= 0)
        {
            // TODO: trigger death scene, reload level, reset health
        }
    }

    void TriggerRecoil(Vector2 hitPosition)
    {
        if (isRecoiling) return;

        Vector2 recoilDir = ((Vector2)transform.position - hitPosition).normalized;
        StartCoroutine(RecoilRoutine(recoilDir));
    }

    IEnumerator RecoilRoutine(Vector2 direction)
    {
        isRecoiling = true;
        
        float timer = 0;
        while (timer < recoilTime)
        {
            // Player's movement is not affected by input
            rb.linearVelocity = direction * recoilSpeed; 
            timer += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        isRecoiling = false;
    }

    void BeginInvincibity()
    {
        if (isInvincible) return;
        StartCoroutine(InvincibleRoutine());
    }

    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        
        float timer = 0f;
        bool visible = false;
        while (timer < invincibleTime)
        {
            // Flash player sprite
            visible = !visible;
            sr.enabled = visible;

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
        sr.enabled = true;
        isInvincible = false;
    }

    public int GetHealth()
    {
        return curHealth;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}
