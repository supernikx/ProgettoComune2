﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Walker : EnemyBase
{
    bool CanShot;

    private float velocityXSmoothing;

    private float HorizontalVelocityRoaming()
    {
        if (transform.position.x > path[nextWaypoint] + WaypointOffset)
        {
            direction = -1;
        }
        else if (transform.position.x < path[nextWaypoint] - WaypointOffset)
        {
            direction = 1;
        }
        else
        {
            nextWaypoint += 1;
            if (path.Length == nextWaypoint)
            {
                nextWaypoint = 0;
            }
        }

        return direction * movementSpeed;
    }

    private float HorizontalVelocityAlert()
    {
        if (transform.position.x > viewCtrl.GetPlayerInRadius().position.x)
        {
            direction = -1;
        }
        else if (transform.position.x < viewCtrl.GetPlayerInRadius().position.x)
        {
            direction = 1;
        }

        return direction * movementSpeed;
    }

    public override void Init(EnemyManager _enemyMng)
    {
        base.Init(_enemyMng);
        CanShot = true;
    }

    private IEnumerator FiringRateCoroutine()
    {
        CanShot = false;
        yield return new WaitForSeconds(1 / enemyShotSettings.firingRate);
        CanShot = true;
    }

    #region API
    Vector3 movementVelocity;
    public override void MoveRoaming()
    {
        //transform.DOPath(GetWaypoints(), 5).SetOptions(true, AxisConstraint.Y).SetLoops(-1).SetEase(Ease.Linear);
        movementVelocity = movementCtrl.GravityCheck();
        movementVelocity.x = Mathf.SmoothDamp(0, HorizontalVelocityRoaming(), ref velocityXSmoothing, (collisionCtrl.collisions.below ? AccelerationTimeOnGround : AccelerationTimeOnAir));
        transform.Translate(collisionCtrl.CheckMovementCollisions(movementVelocity * Time.deltaTime));
        if (collisionCtrl.collisions.right || collisionCtrl.collisions.left)
        {
            nextWaypoint += 1;
            if (path.Length == nextWaypoint)
            {
                nextWaypoint = 0;
            }
        }
    }

    public override void AlertActions()
    {
        Transform target = viewCtrl.GetPlayerInRadius();
        if (target == null)
            return;

        IBullet bullet = PoolManager.instance.GetPooledObject(enemyShotSettings.bulletType, gameObject).GetComponent<IBullet>();
        if ((bullet as ParabolicBullet).CheckShotRange(target.position, shotPosition, enemyShotSettings.shotSpeed))
        {
            if (CanShot)
            {
                bullet.Shot(enemyShotSettings.damage, enemyShotSettings.shotSpeed, 5f, shotPosition, target);
                StartCoroutine(FiringRateCoroutine());
            }
        }
        else
        {
            movementVelocity = movementCtrl.GravityCheck();
            movementVelocity.x = Mathf.SmoothDamp(0, HorizontalVelocityAlert(), ref velocityXSmoothing, (collisionCtrl.collisions.below ? AccelerationTimeOnGround : AccelerationTimeOnAir));
            transform.Translate(collisionCtrl.CheckMovementCollisions(movementVelocity * Time.deltaTime));
        }
    }
    #endregion

}
