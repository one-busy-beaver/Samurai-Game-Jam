using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public static Transform PlayerTarget; // player's location

    [Header("Bullet Settings")]
    [SerializeField] BulletVolley volley;
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] float spawnDisplacement; // Units forward along the barrel

    [Header("Interval Settings")]
    [SerializeField] float burstInterval = 0.5f;
    [SerializeField] int burstCount = 3;
    [SerializeField] float breakInterval = 1f;
    [SerializeField] int maxBreakCount = 5;

    [Header("Aiming Settings")]
    [SerializeField] bool aimsPlayer = true;
    [SerializeField] bool alwaysAimsPlayer;
    [SerializeField] float turnSpeed = 60f;

    [Header("Visual & Facing Settings")]
    [Tooltip("Check this if your sprite assets are drawn facing LEFT in the source PNG files.")]
    [SerializeField] private bool spriteFacesLeftByDefault = true;

    [Header("Walk Settings")]
    [SerializeField] bool canWalk;
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] Vector2 startPos; // Place to start firing
    [SerializeField] float stopDistance = 4f; // Between shooter and player
    [SerializeField] float stopThreshold = 0.05f;

    [Header("Spawn Origin")]
    [SerializeField] Transform firePoint; // spawn location

    enum State { Entering, Fighting, Exiting }

    bool isFiring;
    bool isBursting;
    int breakCount;
    Vector2 exitPos; // Initial spawning and final location (off screen)
    State state = State.Entering;

    // Internal aim & visual tracking
    private Quaternion currentAimRotation = Quaternion.identity;
    private Vector3 initialScale;
    private bool isFacingRight = true;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void Start()
    {
        exitPos = transform.position;

        if (PlayerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                PlayerTarget = player.transform;
        }

        // Keep root upright
        transform.rotation = Quaternion.identity;

        if (aimsPlayer)
            Aim(true);
    }

    void Update()
    {
        HandleWalk();
        if (state != State.Fighting)
            return;

        if (alwaysAimsPlayer)
            Aim();
        else if (aimsPlayer && !isBursting)
            Aim();

        HandleFire();
    }

    void HandleWalk()
    {
        switch (state)
        {
            case State.Entering:
                Aim();
                if (WalkTowards(startPos))
                    state = State.Fighting;
                break;

            case State.Fighting:
                if (!canWalk || isBursting) return;

                Vector2 toPlayer = (Vector2)PlayerTarget.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= stopDistance) return;

                WalkTowards((Vector2)PlayerTarget.position, stopDistance);
                break;

            case State.Exiting:
                if (WalkTowards(exitPos))
                    Destroy(gameObject);
                break;
        }
    }

    bool WalkTowards(Vector2 target, float buffer = 0f)
    {
        Vector2 current = transform.position;
        Vector2 offset = target - current;
        float remaining = offset.magnitude - buffer;

        if (remaining <= stopThreshold) return true;

        float step = Mathf.Min(walkSpeed * Time.deltaTime, remaining);
        transform.position = current + offset.normalized * step;
        return false;
    }

    void Aim(bool snap = false)
    {
        if (PlayerTarget == null) return;

        Vector2 direction = (Vector2)PlayerTarget.position - (Vector2)transform.position;
        if (direction == Vector2.zero) return;

        // 1. Calculate trajectory angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        if (snap)
            currentAimRotation = targetRotation;
        else
            currentAimRotation = Quaternion.RotateTowards(currentAimRotation, targetRotation, turnSpeed * Time.deltaTime);

        // 2. Derive facing strictly from the current aim vector
        Vector2 aimHeading = (Vector2)(currentAimRotation * Vector3.right);
        UpdateFacingDirection(aimHeading.x >= 0f);
    }

    void UpdateFacingDirection(bool faceRight)
    {
        isFacingRight = faceRight;

        // If drawn facing left, flip when facing right; otherwise flip when facing left
        bool shouldFlip = spriteFacesLeftByDefault ? faceRight : !faceRight;

        // Flipping parent's localScale.x cleanly mirrors the body, the gun sprite, and firePoint together
        Vector3 scale = transform.localScale;
        scale.x = shouldFlip ? -Mathf.Abs(initialScale.x) : Mathf.Abs(initialScale.x);
        transform.localScale = scale;
    }

    void HandleFire()
    {
        if (!isFiring)
            StartCoroutine(FireRoutine());
    }

    IEnumerator FireRoutine()
    {
        isFiring = true;
        isBursting = true;
        for (int i = 0; i < burstCount; i++)
        {
            Fire();
            yield return new WaitForSeconds(burstInterval);
        }
        isBursting = false;

        breakCount++;
        if (breakCount >= maxBreakCount)
        {
            state = State.Exiting;
            isFiring = false;
            yield break;
        }

        yield return new WaitForSeconds(breakInterval);
        isFiring = false;
    }

    void Fire()
    {
        List<BulletSpawnInfo> spawns = volley.Generate();

        Vector2 originPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Quaternion originRot = currentAimRotation;

        for (int i = 0; i < spawns.Count; i++)
        {
            BulletSpawnInfo info = spawns[i];

            // Bullet orientation along the aim rotation
            Quaternion bulletRot = originRot * Quaternion.Euler(0, 0, info.angle);

            // Vector3.right is world forward angle (0 degrees) rotated by bulletRot
            Vector2 forwardDir = (Vector2)(bulletRot * Vector3.right);

            Vector2 worldPos = originPos + (Vector2)(originRot * info.relativePosition);
            Vector2 finalPos = worldPos + (forwardDir * spawnDisplacement);

            Bullet bullet = Instantiate(bulletPrefab, finalPos, bulletRot);
            bullet.Motion = info.motion;
            bullet.Heading = forwardDir; // Fires forward along true aim trajectory
        }
    }

    void OnBecameInvisible()
    {
        if (state == State.Exiting)
            Destroy(gameObject);
    }
}