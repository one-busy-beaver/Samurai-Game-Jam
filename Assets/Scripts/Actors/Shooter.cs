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
    [SerializeField] Vector2 spawnDisplacement;

    [Header("Interval Settings")]
    [SerializeField] float burstInterval = 0.5f;
    [SerializeField] int burstCount = 3;
    [SerializeField] float breakInterval = 1f;

    [Header("Aiming Settings")]
    [SerializeField] bool aimsPlayer = true;
    [SerializeField] bool alwaysAimsPlayer;
    [SerializeField] float turnSpeed = 60f;

    Vector2 facing = Vector2.right;
    bool isFiring;
    bool isBursting;

    void Update()
    {
        Walk();
        if (alwaysAimsPlayer)
            Aim(false);
        else if (aimsPlayer)
            Aim(isBursting);
        HandleFire();
    }

    // TODO: implement this
    void Walk(){}
    void Aim(bool pauseCondition)
    {
        Vector2 direction = (Vector2)PlayerTarget.position - (Vector2)transform.position;
        if (direction == Vector2.zero || pauseCondition) return;

        // Calculate angle in degrees
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
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

            // Custom positional adjustment
            Vector2 displacementOffset = (Vector2)(bulletRot * Vector3.right) * spawnDisplacement;
            Vector2 finalPos = worldPos + displacementOffset;

            Bullet bullet = Instantiate(bulletPrefab, finalPos, bulletRot);
            bullet.Motion = info.motion;
            bullet.Heading = (Vector2)(bulletRot * facing);
        }
    }
}