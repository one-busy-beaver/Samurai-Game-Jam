using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] BulletVolley volley;
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] float interval;
    Vector2 facing = Vector2.right;
    bool isFiring;
    

    void Update()
    {
        Walk();
        Aim();
        HandleFire();
    }

    // TODO: implement these
    void Walk(){}
    void Aim(){}

    void HandleFire()
    {
        if (!isFiring)
            StartCoroutine(FireRoutine());
    }

    IEnumerator FireRoutine()
    {
        isFiring = true;
        Fire();
        yield return new WaitForSeconds(interval);
        isFiring = false;
    }

    public void Fire()
    {
        List<BulletSpawnInfo> spawns = volley.Generate();

        for (int i = 0; i < spawns.Count; i++)
        {
            BulletSpawnInfo info = spawns[i];

            Vector2 worldPos = (Vector2)transform.position + (Vector2)(transform.rotation * info.relativePosition);
            Quaternion bulletRot = transform.rotation * Quaternion.Euler(0, 0, info.angle);

            Bullet bullet = Instantiate(bulletPrefab, worldPos, bulletRot);
            bullet.Motion = info.motion;
            bullet.Heading = (Vector2)(bulletRot * facing);
        }
    }
}