using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] float moveSpeed;
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;

    // Player components
    Rigidbody2D rb;

    // Private variables
    Vector2 moveInput;
    Vector2 lastMoveDirection;
    private bool canDash = true;
    private bool dashPressed;

    // PLayer state
    private bool isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

        moveInput = moveInput.normalized;

        dashPressed = false;
        if (keyboard.spaceKey.isPressed) dashPressed = true;
        
    }

    void Move()
    {
        if (moveInput.magnitude != 0) lastMoveDirection = moveInput;
        if (!isDashing)
            rb.linearVelocity = new Vector2(moveSpeed * moveInput.x, moveSpeed * moveInput.y);
    }

    void HandleDash()
    {
        if (dashPressed && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        Vector2 dashDir = moveInput;
        if (dashDir == Vector2.zero) dashDir = lastMoveDirection;
        
        rb.linearVelocity = dashDir * dashSpeed; 

        float timer = 0f;
        while (timer < dashTime)
        {
            if (moveInput.magnitude != 0)
                rb.linearVelocity = moveInput * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
