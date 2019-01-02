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
    protected Vector3 startPosition;

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
        startPosition = transform.position;

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

    /// <summary>
    /// Funzione che imposta il Path del nemico
    /// </summary>
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
        if (enemySM.GoToDeath != null)
            enemySM.GoToDeath();
    }

    /// <summary>
    /// Funzione che reimposta la posizione del nemico con quella iniziale
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    #region Getters
    /// <summary>
    /// Get stun duration
    /// </summary>
    public int GetStunDuration()
    {
        return stunDuration;
    }

    /// <summary>
    /// Get Death Duration
    /// </summary>
    public int GetDeathDuration()
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

    /// <summary>
    /// Funzione che ritorna la direzione del nemico
    /// </summary>
    /// <returns></returns>
    public int GetDirection()
    {
        return direction;
    }

    /// <summary>
    /// Get Graphics Reference
    /// </summary>
    public GameObject GetGraphics()
    {
        return graphics;
    }

    /// <summary>
    /// Funzione che ritorna il parent del nemico
    /// </summary>
    /// <returns></returns>
    public Transform GetEnemyParent()
    {
        return enemyMng.GetEnemyParent();
    }

    /// <summary>
    /// Get Collider Reference
    /// </summary>
    public Collider GetCollider()
    {
        return GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// Funzione che ritorna il Tollerance Bar Controller
    /// </summary>
    /// <returns></returns>
    public EnemyToleranceController GetToleranceCtrl()
    {
        return toleranceCtrl;
    }

    /// <summary>
    /// Funzione che ritorna il Movement Controller
    /// </summary>
    /// <returns></returns>
    public EnemyMovementController GetMovementCtrl()
    {
        return movementCtrl;
    }

    /// <summary>
    /// Funzione che ritorna il Collision Controller
    /// </summary>
    /// <returns></returns>
    public EnemyCollisionController GetCollisionCtrl()
    {
        return collisionCtrl;
    }

    /// <summary>
    /// Funzione che ritorna il View Controller
    /// </summary>
    /// <returns></returns>
    public EnemyViewController GetViewCtrl()
    {
        return viewCtrl;
    }
    #endregion
    #endregion
}
