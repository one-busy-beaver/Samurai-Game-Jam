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

    [Header("Walk Settings")]
    [SerializeField] bool canWalk;
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] Vector2 startPos; // Place to start firing
    [SerializeField] float stopDistance = 4f; // Between shooter and player
    [SerializeField] float stopThreshold = 0.05f;

    enum State { Entering, Fighting, Exiting }

    Vector2 facing = Vector2.right; 
    bool isFiring;
    bool isBursting;
    int breakCount;
    Vector2 exitPos; // Initial spawning and final location (off screen)
    State state = State.Entering;

    void Start()
    {
        exitPos = transform.position;

        if (PlayerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                PlayerTarget = player.transform;
        }
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
            // Move to firing position
            case State.Entering:
            Aim();
                if (WalkTowards(startPos))
                    state = State.Fighting;
                break;

            // Move towards player only between bursts
            case State.Fighting:
                if (!canWalk || isBursting) return;

                Vector2 toPlayer = (Vector2)PlayerTarget.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= stopDistance) return;

                WalkTowards((Vector2)PlayerTarget.position, stopDistance);
                break;

            // Retreat to spawning location; OnBecameInvisible despawns it on the way out
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
        Vector2 direction = (Vector2)PlayerTarget.position - (Vector2)transform.position;
        if (direction == Vector2.zero) return;

        // Calculate angle in degrees
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        if (snap) 
            transform.rotation = targetRotation;
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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

        for (int i = 0; i < spawns.Count; i++)
        {
            BulletSpawnInfo info = spawns[i];

            // Position and rotation information from BulletVolley
            Vector2 worldPos = (Vector2)transform.position + (Vector2)(transform.rotation * info.relativePosition);
            Quaternion bulletRot = transform.rotation * Quaternion.Euler(0, 0, info.angle);

            // Push forward along the bullet's own heading, so the offset follows
            // the barrel regardless of how the shooter is rotated
            Vector2 displacementOffset = (Vector2)(bulletRot * Vector3.right) * spawnDisplacement;
            Vector2 finalPos = worldPos + displacementOffset;

            Bullet bullet = Instantiate(bulletPrefab, finalPos, bulletRot);
            bullet.Motion = info.motion;
            bullet.Heading = (Vector2)(bulletRot * facing);
        }
    }

    void OnBecameInvisible()
    {
        if (state == State.Exiting)
            Destroy(gameObject);
    }
}