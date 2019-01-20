﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerCollisionController : MonoBehaviour, ISticky
{
    #region Delegates
    public Action OnStickyCollision;
    #endregion

    [Header("Collision Settings")]
    [SerializeField]
    /// <summary>
    /// Layer per le collisioni degli oggetti
    /// </summary>
    private LayerMask collisionLayer;
    [SerializeField]
    /// <summary>
    /// Layer per le collisioni dei nemici
    /// </summary>
    private LayerMask enemyLayer;
    [SerializeField]
    /// <summary>
    /// Variabile che definisce il tempo di immunità del player se entra in collisione con un nemico
    /// </summar
    private float immunityDuration;

    [SerializeField]
    /// <summary>
    /// Numero di raycast orrizontali
    /// </summary>
    private int horizontalRayCount = 4;
    /// <summary>
    /// Spazio tra un raycast orrizontale e l'altro
    /// </summary>
    private float horizontalRaySpacing;

    [SerializeField]
    /// <summary>
    /// Numero di raycast orrizontali
    /// </summary>
    private int verticalRayCount = 4;
    /// <summary>
    /// Spazio tra un raycast verticale e l'altro
    /// </summary>
    private float verticalRaySpacing;

    /// <summary>
    /// Offset del bound del collider
    /// </summary>
    private float collisionOffset = 0.015f;

    /// <summary>
    /// Riferimento al player
    /// </summary>
    Player player;

    private Collider colliderToCheck;
    private Collider playerCollider;
    private RaycastStartPoints raycastStartPoints;
    private CollisionInfo collisions;

    bool checkDamageCollisions;

    #region API
    /// <summary>
    /// Funzione che inizializza lo script
    /// </summary>
    public void Init(Player _player)
    {
        player = _player;

        //Prendo le referenze ai component
        playerCollider = GetComponent<Collider>();
        colliderToCheck = playerCollider;

        //Calcolo lo spazio tra i raycast
        CalculateRaySpacing();

        checkDamageCollisions = true;
    }

    /// <summary>
    /// Funzione che controlla se il player va in collisione con qualcosa durante il movimento
    /// </summary>
    /// <param name="_movementVelocity"></param>
    public Vector3 CheckMovementCollisions(Vector3 _movementVelocity)
    {
        //Aggiorna le posizioni da cui partiranno i raycast
        UpdateRaycastOrigins();
        //Reset delle collisioni attuali
        collisions.Reset();

        //Controllo se sono in collisione con oggetti sticky sull'asse orrizontale
        if (collisions.HorizontalStickyCollision())
        {
            //Se si controllo se entro in collisione con qualcosa
            HorizontalCollisions(ref _movementVelocity, true);
        }
        else if (_movementVelocity.x != 0)
        {
            //Se mi sto muovendo sull'asse X controllo se entro in collisione con qualcosa
            HorizontalCollisions(ref _movementVelocity, false);
        }

        //Controllo se sono in collisione con oggetti sticky sull'asse verticale
        if (collisions.VerticalStickyCollision())
        {
            //Se si controllo se entro in collisione con qualcosa
            VerticalCollisions(ref _movementVelocity, true);
        }
        else if (_movementVelocity.y != 0)
        {
            //Se mi sto muovendo sull'asse Y controllo se entro in collisione con qualcosa
            VerticalCollisions(ref _movementVelocity, false);
        }

        return _movementVelocity;
    }

    /// <summary>
    /// Funzione che ricalcola le collisioni sul collider del nemico
    /// </summary>
    public void CalculateParasiteCollision(IEnemy _enemy)
    {
        Collider _enemyCollider = _enemy.gameObject.GetComponent<Collider>();
        playerCollider.enabled = false;
        colliderToCheck = _enemyCollider;
        CalculateRaySpacing();
    }

    /// <summary>
    /// Funzione che ricalcola le collisioni sul collider normale del player
    /// </summary>
    public void CalculateNormalCollision()
    {
        playerCollider.enabled = true;
        colliderToCheck = playerCollider;
        CalculateRaySpacing();
    }

    #region Getter
    /// <summary>
    /// Funzione che ritorna le informazioni sulle collisioni
    /// </summary>
    /// <returns></returns>
    public CollisionInfo GetCollisionInfo()
    {
        return collisions;
    }

    /// <summary>
    /// Funzione che ritorna il tempo di immunità del player
    /// </summary>
    /// <returns></returns>
    public float GetImmunityDuration()
    {
        return immunityDuration;
    }
    #endregion

    #region Setter
    /// <summary>
    /// Funzione che imposta il bool checkDamageCollision con il valore passato come parametro
    /// </summary>
    /// <param name="_check"></param>
    public void CheckDamageCollision(bool _check)
    {
        checkDamageCollisions = _check;
    }
    #endregion
    #endregion

    #region Collision
    /// <summary>
    /// Funzione che calcola lo spazio tra i raycast sia verticali che orrizontali
    /// </summary>
    private void CalculateRaySpacing()
    {
        Bounds bounds = colliderToCheck.bounds;
        bounds.Expand(collisionOffset * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /// <summary>
    /// Funzione che calcola i 4 punti principali da cui partono i raycast
    /// AltoDestra, AltoSinistra, BassoDestra, BassoSinistra
    /// </summary>
    private void UpdateRaycastOrigins()
    {
        Bounds bounds = colliderToCheck.bounds;
        bounds.Expand(collisionOffset * -2);

        raycastStartPoints.bottomLeft = new Vector3(bounds.min.x, bounds.min.y, transform.position.z);
        raycastStartPoints.bottomRight = new Vector3(bounds.max.x, bounds.min.y, transform.position.z);
        raycastStartPoints.topLeft = new Vector3(bounds.min.x, bounds.max.y, transform.position.z);
        raycastStartPoints.topRight = new Vector3(bounds.max.x, bounds.max.y, transform.position.z);
    }

    /// <summary>
    /// Funzione che controlla se avviene una collisione sugli assi verticali
    /// </summary>
    /// <param name="_movementVelocity"></param>
    private void VerticalCollisions(ref Vector3 _movementVelocity, bool _sticky)
    {
        if (_sticky)
        {
            //Se sono in collisione con un oggetto sticky controllo se posso ricevere danni
            if (!checkDamageCollisions)
                return;

            //Determina la lunghezza del raycast
            float rayLenght = 0.3f;
            //Azzero la velocity sull'asse delle y
            _movementVelocity.y = 0f;
            //Cicla tutti i punti da cui deve partire un raycast sull'asse verticale
            for (int i = 0; i < verticalRayCount; i++)
            {
                //Determina il punto da cui deve partire il ray
                Vector3 rayOrigin = raycastStartPoints.bottomLeft;
                rayOrigin += Vector3.right * (verticalRaySpacing * i);

                //Crea il ray
                Ray ray = new Ray(rayOrigin, Vector3.up);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, rayLenght, enemyLayer))
                {
                    //Mando l'evento
                    if (player.OnEnemyCollision != null)
                        player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                }

                //Determina il punto da cui deve partire il ray opposto
                rayOrigin = raycastStartPoints.topLeft;
                rayOrigin += Vector3.right * (verticalRaySpacing * i);

                //Creo il ray opposto
                Ray oppositeRay = new Ray(rayOrigin, Vector3.up);

                if (Physics.Raycast(oppositeRay, out hit, rayLenght, enemyLayer))
                {
                    //Mando l'evento
                    if (player.OnEnemyCollision != null)
                        player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                }
            }
        }
        else
        {
            //Rileva la direzione in cui si sta andando
            float directionY = Mathf.Sign(_movementVelocity.y);
            //Determina la lunghezza del raycast
            float rayLenght = Mathf.Abs(_movementVelocity.y) + collisionOffset;

            //Cicla tutti i punti da cui deve partire un raycast sull'asse verticale
            for (int i = 0; i < verticalRayCount; i++)
            {
                //Determina il punto da cui deve partire il ray
                Vector3 rayOrigin = (directionY == -1) ? raycastStartPoints.bottomLeft : raycastStartPoints.topLeft;
                rayOrigin += Vector3.right * (verticalRaySpacing * i + _movementVelocity.x);

                //Crea il ray
                Ray ray = new Ray(rayOrigin, Vector3.up * directionY);
                RaycastHit hit;

                //Eseguo il raycast
                if (Physics.Raycast(ray, out hit, rayLenght, collisionLayer))
                {
                    //Se colpisco qualcosa sul layer di collisione azzero la velocity
                    _movementVelocity.y = (hit.distance - collisionOffset) * directionY;
                    rayLenght = hit.distance;

                    //Assegno la direzione della collisione al CollisionInfo
                    collisions.above = directionY == 1;
                    collisions.below = directionY == -1;
                }

                if (checkDamageCollisions)
                {
                    if (Physics.Raycast(ray, out hit, rayLenght, enemyLayer))
                    {
                        //Se colpisco un nemico azzero la velocity
                        _movementVelocity.y = (hit.distance - collisionOffset) * directionY;
                        rayLenght = hit.distance;

                        //Mando l'evento
                        if (player.OnEnemyCollision != null)
                            player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                    }
                }
                Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLenght, Color.red);
            }
        }
    }

    /// <summary>
    /// Funzione che controlla se avviene una collisione sugli assi orizzontali
    /// </summary>
    /// <param name="_movementVelocity"></param>
    private void HorizontalCollisions(ref Vector3 _movementVelocity, bool _sticky)
    {        
        if (_sticky)
        {
            //Se sono in collisione con un oggetto sticky controllo se posso ricevere danni
            if (!checkDamageCollisions)
                return;

            //Determino la lunghezza del ray
            float rayLenght = 0.3f;
            //Azzero la velocity sull'asse delle X
            _movementVelocity.x = 0f;
            //Cicla tutti i punti da cui deve partire un raycast sull'asse orizzontale
            for (int i = 0; i < horizontalRayCount; i++)
            {
                //Determina il punto da cui deve partire il ray
                Vector3 rayOrigin = raycastStartPoints.bottomLeft;
                rayOrigin += Vector3.up * (horizontalRaySpacing * i);

                //Crea il ray
                Ray ray = new Ray(rayOrigin, Vector3.right);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, rayLenght, enemyLayer))
                {
                    //Mando l'evento
                    if (player.OnEnemyCollision != null)
                        player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                }

                //Determina il punto da cui deve partire il ray opposto
                rayOrigin = raycastStartPoints.bottomRight;
                rayOrigin += Vector3.up * (horizontalRaySpacing * i);

                //Creo il ray opposto
                Ray oppositeRay = new Ray(rayOrigin, Vector3.right);

                if (Physics.Raycast(oppositeRay, out hit, rayLenght, enemyLayer))
                {
                    //Mando l'evento
                    if (player.OnEnemyCollision != null)
                        player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                }
            }
        }
        else
        {
            //Rileva la direzione in cui si sta andando
            float directionX = Mathf.Sign(_movementVelocity.x);
            //Determina la lunghezza del raycast
            float rayLenght = Mathf.Abs(_movementVelocity.x) + collisionOffset;

            //Cicla tutti i punti da cui deve partire un raycast sull'asse orizzontale
            for (int i = 0; i < horizontalRayCount; i++)
            {
                //Determina il punto da cui deve partire il ray
                Vector3 rayOrigin = (directionX == -1) ? raycastStartPoints.bottomLeft : raycastStartPoints.bottomRight;
                rayOrigin += Vector3.up * (horizontalRaySpacing * i);

                //Crea il ray
                Ray ray = new Ray(rayOrigin, Vector3.right * directionX);
                RaycastHit hit;

                //Eseguo il raycast
                if (Physics.Raycast(ray, out hit, rayLenght, collisionLayer))
                {
                    //Se colpisco qualcosa sul layer di collisione azzero la velocity
                    _movementVelocity.x = (hit.distance - collisionOffset) * directionX;
                    rayLenght = hit.distance;

                    //Assegno la direzione della collisione al CollisionInfo
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }

                if (checkDamageCollisions)
                {
                    if (Physics.Raycast(ray, out hit, rayLenght, enemyLayer))
                    {
                        //Se colpisco un nemico azzero la velocity
                        _movementVelocity.x = (hit.distance - collisionOffset) * directionX;
                        rayLenght = hit.distance;

                        //Mando l'evento
                        if (player.OnEnemyCollision != null)
                            player.OnEnemyCollision(hit.collider.GetComponent<IEnemy>());
                    }
                }
                Debug.DrawRay(rayOrigin, Vector3.right * directionX * rayLenght, Color.red);
            }
        }
    }

    /// <summary>
    /// Funzione chiamata quando si entra in collisione con un oggetto sticky
    /// </summary>
    /// <param name="_direction"></param>
    public void OnSticky(Direction _direction)
    {
        collisions.ResetStickyCollision();
        switch (_direction)
        {
            case Direction.Left:
                collisions.leftStickyCollision = true;
                break;
            case Direction.Right:
                collisions.rightStickyCollision = true;
                break;
            case Direction.Above:
                collisions.aboveStickyCollision = true;
                break;
            case Direction.Below:
                collisions.belowStickyCollision = true;
                break;
            case Direction.None:
                Debug.LogWarning("Direzione di collisione non prevista");
                break;
        }

        //Lanzio l'evento OnStickycollision
        if (OnStickyCollision != null)
            OnStickyCollision();
    }

    /// <summary>
    /// Funzione chiamata quando non si è più in collisione con un oggetto sticky
    /// </summary>
    public void OnStickyEnd()
    {
        collisions.ResetStickyCollision();
    }

    /// <summary>
    /// Struttura che contiene le coordinate dei 4 punti principali da cui partono i ray
    /// che controllano le collisioni
    /// </summary>
    private struct RaycastStartPoints
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
    }

    /// <summary>
    /// Struttura che contiene le informazioni sulla collisione attuale
    /// </summary>
    public struct CollisionInfo
    {
        public bool above;
        public bool below;
        public bool left;
        public bool right;

        public bool leftStickyCollision;
        public bool rightStickyCollision;
        public bool aboveStickyCollision;
        public bool belowStickyCollision;

        public void Reset()
        {
            above = false;
            below = false;
            left = false;
            right = false;
        }

        public bool StickyCollision()
        {
            if (leftStickyCollision || rightStickyCollision || aboveStickyCollision || belowStickyCollision)
                return true;
            return false;
        }

        public bool HorizontalStickyCollision()
        {
            if (leftStickyCollision || rightStickyCollision)
                return true;
            return false;
        }

        public bool VerticalStickyCollision()
        {
            if (aboveStickyCollision || belowStickyCollision)
                return true;
            return false;
        }

        public void ResetStickyCollision()
        {
            aboveStickyCollision = false;
            belowStickyCollision = false;
            leftStickyCollision = false;
            rightStickyCollision = false;
        }
    }
    #endregion
}
