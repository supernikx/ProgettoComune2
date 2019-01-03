﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour, IPoolObject, IBullet
{
    #region IPoolObject
    public GameObject ownerObject
    {
        get
        {
            return _ownerObject;
        }
        set
        {
            _ownerObject = value;
        }
    }
    GameObject _ownerObject;

    public State CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            _currentState = value;
        }
    }
    State _currentState;

    public event PoolManagerEvets.Events OnObjectSpawn;
    public event PoolManagerEvets.Events OnObjectDestroy;
    #endregion
    /// <summary>
    /// Danno del proiettile
    /// </summary>
    protected int damage;
    /// <summary>
    /// Distanza massima che può percorrere il proiettile
    /// </summary>
    protected float range;
    /// <summary>
    /// Velocità a cui va il proiettile
    /// </summary>
    protected float speed;
    /// <summary>
    /// Direzione in cui va il proiettile
    /// </summary>
    protected Vector3? shotDirection;
    /// <summary>
    /// Posizione del target
    /// </summary>
    protected Vector3? targetPosition;
    /// <summary>
    /// Angolo del fucile
    /// </summary>
    protected float shotAngle;
    /// <summary>
    /// Posizione da cui parte lo sparo
    /// </summary>
    protected Transform shotPosition;

    /// <summary>
    /// Funzione di Setup
    /// </summary>
    public void Setup()
    {
        bulletCollider = GetComponent<Collider>();
        CalculateRaySpacing();
    }

    /// <summary>
    /// Funzione che richiama l'evento di spawn del proiettile
    /// </summary>
    protected void ObjectSpawnEvent()
    {
        if (OnObjectSpawn != null)
            OnObjectSpawn(this);
    }
    /// <summary>
    /// Funzione che richiama l'evento di Destroy del proiettile
    /// </summary>
    protected void ObjectDestroyEvent()
    {
        if (OnObjectDestroy != null)
            OnObjectDestroy(this);
    }

    /// <summary>
    /// Funzione che gestisce il behaviour del proiettile
    /// </summary>
    protected abstract void Move();

    private void Update()
    {
        Move();
    }

    #region IBullet
    /// <summary>
    /// Funzione che inizializza il proiettile e lo fa sparare
    /// </summary>
    /// <param name="_speed"></param>
    /// <param name="_range"></param>
    /// <param name="_shootPosition"></param>
    /// <param name="_direction"></param>
    public virtual void Shot(int _damage, float _speed, float _range, Transform _shotPosition, Vector3 _direction)
    {
        damage = _damage;
        speed = _speed;
        range = _range;
        shotDirection = _direction;
        shotPosition = _shotPosition;
        targetPosition = null;
        transform.position = shotPosition.position;

        shotAngle = Mathf.Atan2(shotDirection.Value.y, shotDirection.Value.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, shotAngle);
        ObjectSpawnEvent();
    }

    /// <summary>
    /// Funzione che inizializza il proiettile e lo fa sparare ad un target
    /// </summary>
    /// <param name="_speed"></param>
    /// <param name="_shotPosition"></param>
    /// <param name="_target"></param>
    public virtual void Shot(int _damage, float _speed, float _range,Transform _shotPosition, Transform _target)
    {
        damage = _damage;
        speed = _speed;
        range = _range;
        shotPosition = _shotPosition;
        targetPosition = _target.position;
        shotDirection = null;
        transform.position = shotPosition.position;

        ObjectSpawnEvent();
    }

    /// <summary>
    /// Funzione che ritrona il danno che fa il proiettile
    /// </summary>
    /// <returns></returns>
    public int GetBulletDamage()
    {
        return damage;
    }
    #endregion

    #region Collision
    Collider bulletCollider;
    const float colliderOffset = 0.015f;
    [Header("Collision Settings")]
    [SerializeField]
    int rayCount;
    [SerializeField]
    float rayLenght;
    float raySpacing;
    float test;

    /// <summary>
    /// Funzione che calcola lo spazio tra i raycast
    /// </summary>
    private void CalculateRaySpacing()
    {
        Bounds bulletBound = bulletCollider.bounds;
        bulletBound.Expand(colliderOffset * -2f);
        raySpacing = bulletBound.size.y / (rayCount - 1);
        test = bulletBound.size.x / 2;
    }

    /// <summary>
    /// Funzione che controlla se avviene una collisione sugli assi verticali
    /// </summary>
    /// <param name="_movementVelocity"></param>
    protected bool Checkcollisions(Vector3 _direction)
    {
        //Cicla tutti i punti da cui deve partire un raycast
        for (int i = 0; i < rayCount; i++)
        {
            //Determina il punto da cui deve partire il ray (centro del proiettile)
            Vector3 rayOrigin = transform.position - transform.up * (raySpacing * ((rayCount - 1) / 2)) + (transform.right * test);
            rayOrigin += transform.up * (raySpacing * i);
            //Debug.Log(transform.right);

            //Crea il ray
            Ray ray = new Ray(rayOrigin, transform.right);
            RaycastHit hit;

            //Eseguo il raycast
            if (Physics.Raycast(ray, out hit, rayLenght))
            {
                //Se colpisce qualcosa chiama la funzione e ritorna true
                OnBulletCollision(hit);
                return true;
            }

            Debug.DrawRay(rayOrigin, transform.right * rayLenght, Color.red);
        }
        return false;
    }

    /// <summary>
    /// Funzione chiamata quando il proiettile entra in collisione con qualcosa
    /// </summary>
    /// <param name="_collisionInfo"></param>
    protected abstract void OnBulletCollision(RaycastHit _collisionInfo);
    #endregion
}
