﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerShotController : MonoBehaviour
{
    #region Delegates
    private delegate void ShotDelegate();
    ShotDelegate Shot;
    #endregion

    [Header("Shoot Settings")]
    [SerializeField]
    private Transform shotPoint;
    [SerializeField]
    private GameObject gun;
    [SerializeField]
    private float range;
    [SerializeField]
    private float shotSpeed;
    [SerializeField]
    private float firingRate;

    /// <summary>
    /// Referenza al pool manager
    /// </summary>
    PoolManager pool;
    /// <summary>
    /// Boolean che definisce se posso sparare o no
    /// </summary>
    bool canShot;

    void Update()
    {
        if (UseMouseInput())
        {
            MouseAim();

            if (Input.GetButton("LeftMouse"))
            {
                //Controllo se posso sparare
                if (CheckFiringRate())
                {
                    Shot();
                }
            }
        }
        else
        {
            if (JoystickAim())
            {
                //Controllo se posso sparare
                if (CheckFiringRate())
                {
                    Shot();
                }
            }
        }

        //Aumento il contatore del firing rate
        firingRateTimer -= Time.deltaTime;
    }

    #region Aim
    /// <summary>
    /// Funzione che ruota l'arma in base alla posizione del mouse
    /// </summary>
    Vector2 direction;
    private void MouseAim()
    {
        float rotationZ;
        //Converto la posizione da cui devo sparare da world a screen
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(shotPoint.position);
        //Calcolo la direzione tra la posizione del mouse e lo shoot point
        direction = (Input.mousePosition - screenPoint).normalized;
        //Calcolo la rotazione che deve fare il fucile
        rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //Ruoto l'arma nella direzione in cui si sta mirando
        if (direction.x < 0)
        {
            //Se si sta mirando nel senso opposto flippo l'arma
            gun.transform.rotation = Quaternion.Euler(Mathf.PI * Mathf.Rad2Deg, 0.0f, -rotationZ);
        }
        else
        {
            gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
        }
    }

    /// <summary>
    /// Funzione che muove l'arma in base alla direzione del right stick
    /// </summary>
    private bool JoystickAim()
    {
        float rotationZ;
        //Prendo gli input
        Vector2 input = new Vector3(Input.GetAxisRaw("HorizontalJoystickRightStick"), Input.GetAxisRaw("VerticalJoystickRightStick"));
        //Se non muovo lo stick lascio l'arma nella posizione precedente
        if (input.x == 0 && input.y == 0)
            return false;
        //Prendo la direzione a cui devo mirare
        direction = new Vector3(input.x, input.y);
        //Calcolo la rotazione che deve fare il fucile
        rotationZ = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

        //Ruoto l'arma nella direzione in cui si sta mirando
        if (direction.x < 0)
        {
            //Se si sta mirando nel senso opposto flippo l'arma
            gun.transform.rotation = Quaternion.Euler(Mathf.PI * Mathf.Rad2Deg, 0.0f, -rotationZ);
        }
        else
        {
            gun.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
        }
        return true;
    }
    #endregion

    #region Shot
    /// <summary>
    /// Funzione che controlla se posso sparare e ritorna true o false
    /// </summary>
    private float firingRateTimer;
    private bool CheckFiringRate()
    {
        if (firingRateTimer < 0)
        {
            firingRateTimer = 1f / firingRate;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Funzione che prende un proiettile dal pool manager e lo imposta per sparare
    /// </summary>
    private void ShotStunBullet()
    {
        IBullet bullet = pool.GetPooledObject(ObjectTypes.ParabolicBullet, gameObject).GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.Shot(shotSpeed, range, shotPoint, direction);
        }
    }

    /// <summary>
    /// Funzione che prende un proiettile danneggiante dal pool manager e lo imposta per sparare
    /// </summary>
    private void ShotDamageBullet()
    {
        IBullet bullet = pool.GetPooledObject(ObjectTypes.DamageBullet, gameObject).GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.Shot(shotSpeed, range, shotPoint, direction);
        }
    }
    #endregion

    /// <summary>
    /// Funzione che controlla se usare gli input del mouse o del controller
    /// </summary>
    /// <returns></returns>
    Vector3 mousePreviewsPos;
    private bool UseMouseInput()
    {
        if (Mathf.Approximately(Input.mousePosition.x, mousePreviewsPos.x) && Mathf.Approximately(Input.mousePosition.y, mousePreviewsPos.y))
        {
            if (Input.GetJoystickNames().Where(j => j != "").FirstOrDefault() != null)
                return false;
            return true;
        }

        mousePreviewsPos = Input.mousePosition;
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(shotPoint.position, range);
    }

    #region API
    /// <summary>
    /// Funzione che inizializza lo script
    /// </summary>
    public void Init(PoolManager _poolManager)
    {
        pool = _poolManager;
        canShot = false;
    }

    /// <summary>
    /// Funzione che imposta la variabile can shoot
    /// con quella passata come parametro
    /// </summary>
    /// <param name="_canShoot"></param>
    public void SetCanShoot(bool _canShoot)
    {
        canShot = _canShoot;
    }

    /// <summary>
    /// Funzone che cambia il tipo di sparo
    /// </summary>
    bool useStunBullets;
    public void ChangeShotType()
    {
        //Controllo se posso cambiare tipo di sparo
        if (useStunBullets && canShot)
        {
            Shot = ShotDamageBullet;
            useStunBullets = false;
        }
        else
        {
            Shot = ShotStunBullet;
            useStunBullets = true;
        }
    }
    #endregion
}
