﻿using UnityEngine;
using System.Collections;

public class StickyBullet : BulletBase
{
    [Header("Sticky Bullet Settings")]
    [SerializeField]
    private ObjectTypes stickyObjectType;
    [SerializeField]
    private int percentageLife;
    [SerializeField]
    private int timeInSeconds;

    protected override void Move()
    {
        Vector3 _movementDirection = transform.right * speed;
        if (!Checkcollisions(_movementDirection * Time.deltaTime))
        {
            transform.position += _movementDirection * Time.deltaTime;
            if (Vector3.Distance(shotPosition, transform.position) >= range)
            {
                ObjectDestroyEvent();
            }
        }
    }

    protected override bool OnBulletCollision(RaycastHit _collisionInfo)
    {
        if (ownerObject.tag == "Player" && _collisionInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IEnemy enemyHit = _collisionInfo.transform.gameObject.GetComponent<IEnemy>();
            enemyHit.DamageHit(GetBulletDamage());
        }

        if (ownerObject.tag != "Player" && _collisionInfo.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Player player = _collisionInfo.transform.gameObject.GetComponent<Player>();
            if (player != null)
            {
                int damage = Mathf.RoundToInt(player.GetHealthController().GetHealth() * percentageLife / 100);
                player.GetHealthController().DamageHit(damage, timeInSeconds);

            }
            else
            {
                IEnemy enemyHit = _collisionInfo.transform.gameObject.GetComponent<IEnemy>();
                int damage = Mathf.RoundToInt(enemyHit.GetToleranceCtrl().GetTolerance() * percentageLife / 100);
                enemyHit.GetToleranceCtrl().AddTolerance(damage);
            }

            if (player.OnPlayerHit != null)
                player.OnPlayerHit();
        }

        if (ownerObject.tag == "Player" && _collisionInfo.transform.gameObject.layer == LayerMask.NameToLayer("Buttons"))
        {
            IButton _target = _collisionInfo.transform.gameObject.GetComponent<IButton>();
            if (_target.GetTriggerType() == ButtonTriggerType.Shot)
                _target.Activate();
            else
                return false;
        }

        if (_collisionInfo.transform.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            SpawnStickyObject(_collisionInfo.point, _collisionInfo.normal);
        }

        return base.OnBulletCollision(_collisionInfo);
    }

    /// <summary>
    /// Funzione che spawna uno sticky object 
    /// </summary>
    /// <param name="_spawnPosition"></param>
    /// <param name="_normal"></param>
    private void SpawnStickyObject(Vector3 _spawnPosition, Vector3 _normal)
    {
        StickyObject stickyObject = PoolManager.instance.GetPooledObject(stickyObjectType, gameObject).GetComponent<StickyObject>();
        stickyObject.Init(_spawnPosition, Quaternion.LookRotation(Vector3.forward, _normal));

        Vector3 rightMaxPosition = stickyObject.CheckSpace(_normal, 1);
        Vector3 leftMaxPosition = stickyObject.CheckSpace(_normal, -1);
        stickyObject.Spawn(leftMaxPosition, rightMaxPosition);
    }
}
