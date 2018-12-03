﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCollisionController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Header("Ground Settings")]
    /// <summary>
    /// Velocità di movimento
    /// </summary>
    public float MovementSpeed;
    /// <summary>
    /// Velocità di accelerazione e decelerazione se si è a terra
    /// più è bassa più veloce è l'accelerazione
    /// </summary>
    public float AccelerationTimeOnGround;

    [Header("Jump Settings")]
    /// <summary>
    /// Altezza massima raggiungibile in unity unit
    /// </summary>
    public float JumpUnitHeight;
    /// <summary>
    /// Tempo in secondi che ci vuole per raggiungere l'altezza massima del salto
    /// </summary>
    public float JumpTimeToReachTop;
    /// <summary>
    /// Velocità di accelerazione e decelerazione se si è in aria
    /// più è bassa più veloce è l'accelerazione
    /// </summary>
    public float AccelerationTimeOnAir;
    /// <summary>
    /// Variabile che definisce la gravità applicata overtime
    /// se non si è in collisione sopra/sotto
    /// </summary>
    private float gravity;

    /// <summary>
    /// Riferimento al collision controller
    /// </summary>
    private PlayerCollisionController collisionCtrl;
    /// <summary>
    /// Vettore che contiene gli input sull'asse orizzontale e verticale
    /// </summary>
    private Vector2 input;
    /// <summary>
    /// Boolean che definisce se mi posso muovere o meno
    /// </summary>
    bool canMove;

    private void Update()
    {
        if (canMove)
        {
            if (collisionCtrl.collisions.above || collisionCtrl.collisions.below)
            {
                //Se sono in collisione con qualcosa sopra/sotto evito di accumulare gravità
                movementVelocity.y = 0;
            }

            //Leggo input orrizontali e verticali
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            //Controllo se è stato premuto il tasto di salto e se sono a terra
            if (Input.GetButtonDown("Jump") && collisionCtrl.collisions.below)
            {
                Jump();
            }

            AddGravity();

            Move();
        }
    }

    /// <summary>
    /// Funzione che aggiunge gravità al player
    /// </summary>
    private void AddGravity()
    {
        //Aggiungo gravità al player
        movementVelocity.y += gravity * Time.deltaTime;
    }

    private float velocityXSmoothing;
    private Vector3 movementVelocity;
    /// <summary>
    /// Funzione che esegue i calcoli necessari per far muovere il player
    /// </summary>
    private void Move()
    {
        //Eseguo una breve transizione dalla mia velocity attuale a quella successiva
        movementVelocity.x = Mathf.SmoothDamp(movementVelocity.x, (input.x * MovementSpeed), ref velocityXSmoothing, (collisionCtrl.collisions.below ? AccelerationTimeOnGround : AccelerationTimeOnAir));

        //Mi muovo
        transform.Translate(collisionCtrl.CheckMovementCollisions(movementVelocity * Time.deltaTime));
    }

    private float jumpVelocity;
    /// <summary>
    /// Funzione che imposta la velocity dell'asse verticale per saltare
    /// </summary>
    private void Jump()
    {
        movementVelocity.y = jumpVelocity;
    }

    #region API
    /// <summary>
    /// Funzione di inizializzazione del player
    /// </summary>
    public void Init(PlayerCollisionController _collisionCtrl)
    {
        collisionCtrl = _collisionCtrl;
        //Calcolo la gravità
        gravity = -(2 * JumpUnitHeight) / Mathf.Pow(JumpTimeToReachTop, 2);
        //Calcolo la velocità del salto
        jumpVelocity = Mathf.Abs(gravity * JumpTimeToReachTop);

        canMove = false;
    }

    /// <summary>
    /// Funzione che imposta la variabile can move
    /// con la variabile passata come parametro
    /// </summary>
    /// <param name="_canMove"></param>
    public void SetCanMove(bool _canMove)
    {
        canMove = _canMove;
    }
    #endregion
}
