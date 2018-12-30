﻿using UnityEngine;
using System.Collections;
using StateMachine.EnemySM;

[RequireComponent(typeof(EnemyToleranceController))]
[RequireComponent(typeof(EnemyMovementController))]
[RequireComponent(typeof(EnemyCollisionController))]
[RequireComponent(typeof(EnemyViewController))]
public abstract class EnemyBase : MonoBehaviour, IEnemy
{

    [Header("General Movement Settings")]
    protected float movementSpeed;
    [SerializeField]
    protected GameObject Waypoints;
    [SerializeField]
    protected float horizontalVelocity = 0;
    [SerializeField]
    protected float AccelerationTimeOnGround;
    [SerializeField]
    protected float AccelerationTimeOnAir;

    protected float WaypointOffset = 0.5f;
    protected float[] path;
    protected int nextWaypoint;
    [SerializeField]
    protected int direction;

    [Header("Damage Settings")]
    [SerializeField]
    protected int enemyDamage;

    [Header("Stun Settings")]
    [SerializeField]
    protected int stunDuration;

    [Header("Death Settings")]
    [SerializeField]
    protected int deathDuration;

    [Header("Other Settings")]
    [SerializeField]
    protected GameObject graphics;

    protected EnemyManager enemyMng;
    protected EnemySMController enemySM;
    
    protected EnemyToleranceController toleranceCtrl;
    protected EnemyMovementController movementCtrl;
    protected EnemyCollisionController collisionCtrl;
    protected EnemyViewController viewCtrl;

    #region API
    /// <summary>
    /// Initialize Script
    /// </summary>
    public virtual void Init(EnemyManager _enemyMng)
    {
        enemyMng = _enemyMng;

        // Initialize Enemy State Machine
        enemySM = GetComponent<EnemySMController>();
        if (enemySM != null)
            enemySM.Init(this, enemyMng);

        toleranceCtrl = GetComponent<EnemyToleranceController>();
        if (toleranceCtrl != null)
            toleranceCtrl.Init();

        collisionCtrl = GetComponent<EnemyCollisionController>();
        if (collisionCtrl != null)
            collisionCtrl.Init();

        movementCtrl = GetComponent<EnemyMovementController>();
        if (movementCtrl != null)
            movementCtrl.Init(collisionCtrl);

        viewCtrl = GetComponent<EnemyViewController>();
        if (viewCtrl != null)
            viewCtrl.Init();

        SetPath();

    }

    public void SetPath()
    {
        int _childCount = Waypoints.transform.childCount;
        path = new float[_childCount];
        for (int i = 0; i < _childCount; i++)
        {
            path[i] = Waypoints.transform.GetChild(i).position.x;
        }

        nextWaypoint = 0;
    }

    /// <summary>
    /// Funzione che si ovvupa del movimento in stato di roaming
    /// </summary>
    public abstract void MoveRoaming();

    /// <summary>
    /// Funzione che si ovvupa del movimento in stato di alert
    /// Se restituisce false, il player non è più in vista
    /// </summary>
    public abstract bool AlertActions();

    /// <summary>
    /// Funzione che si ovvupa di bloccare il movimento
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Funzione che invia il nemico in stato di allerta
    /// </summary>
    public void Alert()
    {
        if (enemySM.GoToAlert != null)
            enemySM.GoToAlert();
    }

    /// <summary>
    /// Funzione che manda il nemico in stato Stun
    /// </summary>
    public void Stun()
    {
        if (enemySM.GoToStun != null)
            enemySM.GoToStun();
    }

    /// <summary>
    /// Funzione che manda il nemico in stato Morte
    /// </summary>
    public void Die()
    {
        if (enemySM.GoToDeath != null)
            enemySM.GoToDeath();
    }

    /// <summary>
    /// Funzione che manda il nemico in stato parassita
    /// </summary>
    /// <param name="_player"></param>
    public void Parasite(Player _player)
    {
        if (enemySM.GoToParasite != null)
            enemySM.GoToParasite(_player);
    }

    /// <summary>
    /// Funzione che manda il nemico in stato AfterParasite
    /// </summary>
    public void EndParasite()
    {
        if (enemySM.GoToAfterParasite != null)
            enemySM.GoToAfterParasite();
    }

    #region Getters
    /// <summary>
    /// Get stun duration
    /// </summary>
    public virtual int GetStunDuration()
    {
        return stunDuration;
    }

    /// <summary>
    /// Get Death Duration
    /// </summary>
    public virtual int GetDeathDuration()
    {
        return deathDuration;
    }

    /// <summary>
    /// Funzione che ritorna il danno del nemico
    /// </summary>
    /// <returns></returns>
    public int GetDamage()
    {
        return enemyDamage;
    }

    public int GetDirection()
    {
        return direction;
    }

    /// <summary>
    /// Get Graphics Reference
    /// </summary>
    public virtual GameObject GetGraphics()
    {
        return graphics;
    }

    /// <summary>
    /// Get Collider Reference
    /// </summary>
    public virtual CapsuleCollider GetCollider()
    {
        return GetComponent<CapsuleCollider>();
    }

    public virtual EnemyToleranceController GetToleranceCtrl()
    {
        return toleranceCtrl;
    }

    public virtual EnemyMovementController GetMovementCtrl()
    {
        return movementCtrl;
    }

    public virtual EnemyCollisionController GetCollisionCtrl()
    {
        return collisionCtrl;
    }

    public virtual EnemyViewController GetViewCtrl()
    {
        return viewCtrl;
    }
    #endregion
    #endregion
}
