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
    [SerializeField] private Transform firePoint; // bullet spawn point

    [Header("Interval Settings")]
    [SerializeField] float burstInterval;
    [SerializeField] int burstCount;
    [SerializeField] float breakInterval;

    [Header("Aiming Settings")]
    [SerializeField] bool aimsPlayer;
    [SerializeField] float turnSpeed;

    Vector2 facing = Vector2.right;
    bool isFiring;
    

    void Update()
    {
        if (PauseController.IsGamePaused) return;
        Walk();
        if (aimsPlayer)
            Aim();
        HandleFire();
    }

    // TODO: implement this
    void Walk(){}
    void Aim()
    {
        Vector2 direction = (Vector2)PlayerTarget.position - (Vector2)transform.position;
        if (direction == Vector2.zero) return;

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
        for (int i = 0; i < burstCount; i++)
        {
            Fire();
            if (i < burstCount - 1) yield return new WaitForSeconds(burstInterval);
        }
        yield return new WaitForSeconds(breakInterval);
        isFiring = false;
    }

    public void Fire()
    {
        if (bulletPrefab == null || volley == null) return;

        // fallback if firePoint wasn't assigned
        Vector3 basePos = firePoint != null ? firePoint.position : transform.position;
        Quaternion baseRot = firePoint != null ? firePoint.rotation : transform.rotation;

        List<BulletSpawnInfo> spawns = volley.Generate();

        for (int i = 0; i < spawns.Count; i++)
        {
            BulletSpawnInfo info = spawns[i];

            Vector2 worldPos = (Vector2)basePos + (Vector2)(baseRot * info.relativePosition);
            Quaternion bulletRot = baseRot * Quaternion.Euler(0, 0, info.angle);

            Bullet bullet = Instantiate(bulletPrefab, worldPos, bulletRot);
            bullet.Motion = info.motion;
            bullet.Heading = -transform.right;
        }
    }
}