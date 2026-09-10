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

    // Player Sprites
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;

    // Flash Colors
    [SerializeField] private Color damageFlashColor = Color.red;
    private readonly Color normalColor = Color.white;

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
    // public bool IsInvincible { get; private set; }

    private bool isDashInvincible;
    private bool isDamageInvincible;
    public bool IsInvincible => isDashInvincible || isDamageInvincible;
    public int CurrentHealth { get; private set; }

    // Vars to keep player in-bounds
    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    // updating sprite direction
    void UpdateSpriteDirection()
    {
        // player is stationary. don't change facing sprite
        if (moveInput == Vector2.zero) return;

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            // horizontal movement
            if (moveInput.x > 0) sr.sprite = spriteRight;
            else sr.sprite = spriteLeft;
        }
        else
        {
            // vertical movement
            if (moveInput.y > 0) sr.sprite = spriteUp;
            else sr.sprite = spriteDown;
        }
    }

    // Health Manager
    [SerializeField] private HeartManager heartManager; // I added this

    // Awake calls before start
    void Awake()
    {
        // Allow all enemies to know player's location
        // (moved here from Start() so it's set before any enemy's Start() runs)
        Shooter.PlayerTarget = transform;
    }

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
        UpdateSpriteDirection();
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

    void Move()
    {
        // We always want a non-zero dash direction
        if (moveInput.magnitude != 0) lastMoveDirection = moveInput;
        if (isDashing) return;
        rb.linearVelocity = new Vector2(moveSpeed * moveInput.x, moveSpeed * moveInput.y);
    }

    void HandleDash()
    {
        if (dashPressed && canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    // IEnumerator DashRoutine()
    // {
    //     isDashing = true;
    //     IsInvincible = true;
    //     canDash = false;
    //     Vector2 dashDir = moveInput;
    //     if (dashDir == Vector2.zero) dashDir = lastMoveDirection;

    //     // Increase velocity to dash mode
    //     rb.linearVelocity = dashDir * dashSpeed;

    //     float timer = 0f;
    //     while (timer < dashTime)
    //     {
    //         // Can change direction mid-dash (i.e. direction not locked)
    //         if (moveInput.magnitude != 0)
    //             rb.linearVelocity = moveInput * dashSpeed;

    //         timer += Time.deltaTime;
    //         yield return null;
    //     }
    //     isDashing = false;
    //     IsInvincible = false;
    //     yield return new WaitForSeconds(dashCooldown);
    //     canDash = true;
    // }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        isDashInvincible = true;
        canDash = false;

        // Visual feedback: make player semi-transparent while invincible
        Color c = sr.color;
        c.a = 0.5f;
        sr.color = c;

        Vector2 dashDir = moveInput == Vector2.zero ? lastMoveDirection : moveInput;
        rb.linearVelocity = dashDir * dashSpeed;

        float timer = 0f;
        while (timer < dashTime)
        {
            if (moveInput.magnitude != 0)
                rb.linearVelocity = moveInput * dashSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        // Reset visual feedback
        c.a = 1f;
        sr.color = c;

        isDashing = false;
        isDashInvincible = false;

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
            SceneManager.LoadScene("PreDeathScene");

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

    // IEnumerator InvincibleRoutine()
    // {
    //     IsInvincible = true;

    //     float timer = 0f;
    //     while (timer < invincibleTime)
    //     {
    //         // PingPong-ing from 0.0 to 1.0
    //         float flashT = Mathf.PingPong(timer * flashMultiplier, 1f);

    //         // lerp between base white and red
    //         sr.color = Color.Lerp(normalColor, damageFlashColor, flashT);

    //         timer += Time.deltaTime;
    //         yield return null;
    //     }

    //     // reset cleanly back to full whtie
    //     sr.color = normalColor;
    //     IsInvincible = false;
    // }

    IEnumerator InvincibleRoutine()
    {
        isDamageInvincible = true;

        float timer = 0f;
        while (timer < invincibleTime)
        {
            float flashT = Mathf.PingPong(timer * flashMultiplier, 1f);
            sr.color = Color.Lerp(normalColor, damageFlashColor, flashT);

            timer += Time.deltaTime;
            yield return null;
        }

        sr.color = normalColor;
        isDamageInvincible = false;
    }

    private void UpdateCameraBounds()
    {
        if (mainCamera == null) return;

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        minBounds = new Vector2(bottomLeft.x + halfWidth, bottomLeft.y + halfHeight);
        maxBounds = new Vector2(topRight.x - halfWidth, topRight.y - halfHeight);
    }

    private void ClampPositionToCamera()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);

        transform.position = pos;
    }
}