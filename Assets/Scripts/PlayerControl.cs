using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

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
    [SerializeField] float flashMultiplier;

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
    public bool IsInvincible { get; private set; }
    public int CurrentHealth { get; private set; }

    // Health Manager
    [SerializeField] private HeartManager heartManager; // I added this

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        lastMoveDirection = new Vector2(0, 1);
        CurrentHealth = maxHealth;

        if (heartManager != null)
        {
            heartManager.InitializeHearts(maxHealth);
        }
    }

    void Update()
    {
        ReadInput();
        // Move();
        HandleDash();
    }

    void FixedUpdate()
    {
        // keep Move() here to avoid jitter in monitors different from 50Hz
        Move();
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
        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.shiftKey.wasPressedThisFrame) {
            dashPressed = true;
        }
        
    }

    /* MOVEMENTS */

    // Control player's basic movement
    void Move()
    {
        if (moveInput.magnitude != 0) lastMoveDirection = moveInput;
        if (isDashing) return;
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
        IsInvincible = true;
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
        IsInvincible = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    /* HEALTH & DAMAGE */

    // Used by hazards (e.g. bullets) to deal damage
    public void TakeDamage(Vector2 hitPosition, int increment)
    {
        if (IsInvincible) return; // no damage taken

        CurrentHealth = Mathf.Max(0, CurrentHealth - increment);

        if (heartManager != null) // update hearts UI
        {
            heartManager.UpdateHearts(CurrentHealth);
        }
    
        if (CurrentHealth == 0)
        {   
            Debug.Log("you died");

            // switch to death screen
            SceneManager.LoadScene("DeathScene");

            return;

            // TODO: reload level
        }

        TriggerRecoil(hitPosition);
        BeginInvincibity();
    }

    // heal maychance?
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        if (heartManager != null)
        {
            heartManager.UpdateHearts(CurrentHealth);
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
            // Player can not control character's moving direction
            rb.linearVelocity = direction * recoilSpeed; 
            timer += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        isRecoiling = false;
    }

    void BeginInvincibity()
    {
        if (IsInvincible) return;
        StartCoroutine(InvincibleRoutine());
    }

    IEnumerator InvincibleRoutine()
    {
        IsInvincible = true;
        
        float timer = 0f;
        while (timer < invincibleTime)
        {
            // Flash player's sprite by changing alpha
            Color c = sr.color;
            float whatever = Mathf.PingPong(timer * flashMultiplier, 1f);
            c.a = Mathf.Lerp(0.3f, 1f, whatever);
            Debug.Log("pingpong: " + whatever);
            Debug.Log(c.a);
            sr.color = c;
            yield return null;
            timer += Time.deltaTime;
        }
        IsInvincible = false;
        Color c2 = sr.color;
        c2.a = 1f;
        sr.color = c2;
    }
}
