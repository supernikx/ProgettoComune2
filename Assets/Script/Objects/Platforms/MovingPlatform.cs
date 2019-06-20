﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlatformCollisionController))]
public class MovingPlatform : PlatformBase
{
    [Header("Movement Options")]
    [SerializeField]
    private float movingSpd;
    [SerializeField]
    private List<Transform> reachPoints = new List<Transform>();
    private List<Vector3> reachPointsPositions = new List<Vector3>();
    [SerializeField]
    private float waitTime = 0;

    Vector3 nextReachPosition;
    private Vector3 direction;
    bool setupped = false;
    bool canMove = false;
    private PlatformCollisionController collisionCtrl;

    #region API
    public override void Init()
    {
        collisionCtrl = GetComponent<PlatformCollisionController>();
        if (collisionCtrl != null)
            collisionCtrl.Init();

        reachPoints.Add(Instantiate(new GameObject("StartPosition"), transform).transform);
        reachPointsPositions = reachPoints.Select(t => t.position).ToList();

        distanceTraveled = 0;
        setupped = true;
        canMove = true;

        SetNextWaypoint();
    }

    public override void MoveBehaviour()
    {
        if (!setupped || !canMove)
            return;

        //Calcolo il movimento della piattaforma e dei passeggeri
        CalculatePlatformMovement();
        collisionCtrl.CalculatePassengerMovement(moveVelocity);
        collisionCtrl.CalculateAlwaysMovement(moveVelocity);

        //Muovo i passeggeri che devono essere mossi prima della piattaforma
        collisionCtrl.MovePassengers(true);

        //Muovo la piattaforma
        MovePlatform();

        //Muovo i passeggeri che devono essere mossi dopo la piattaforma
        collisionCtrl.MovePassengers(false);
    }

    #endregion

    /// <summary>
    /// Funzione che calcola la direzione in cui deve andare la piattaforma
    /// </summary>
    Vector3 moveVelocity;
    /// <summary>
    /// Distanza percorsa
    /// </summary>
    float distanceTraveled;
    /// <summary>
    /// Distanza da percorre
    /// </summary>
    float distanceToTravel;
    private void CalculatePlatformMovement()
    {
        if (distanceToTravel > distanceTraveled)
        {
            moveVelocity = direction * movingSpd * Time.deltaTime;
            distanceTraveled += movingSpd * Time.deltaTime;
        }
        else
        {
            distanceTraveled = 0;
            transform.position = nextReachPosition;
            StartCoroutine(WaitTime(waitTime));
            SetNextWaypoint();
        }
    }

    /// <summary>
    /// Funzione che muove la piattaforma
    /// </summary>
    private void MovePlatform()
    {
        transform.Translate(moveVelocity);
    }

    int actualPosition = -1;
    /// <summary>
    /// Funzione che aggiorna il waypoint da seguire
    /// </summary>
    private void SetNextWaypoint()
    {
        if (actualPosition == -1 || actualPosition + 1 >= reachPointsPositions.Count)
            actualPosition = 0;
        else
            actualPosition++;

        nextReachPosition = reachPointsPositions[actualPosition];
        direction = (nextReachPosition - transform.position).normalized;
        distanceToTravel = Vector3.Distance(transform.position, nextReachPosition);
    }

    /// <summary>
    /// Funzione che disabilità il movimento per il tempo passato come parametro
    /// </summary>
    /// <param name="_time"></param>
    /// <returns></returns>
    private IEnumerator WaitTime(float _time)
    {
        canMove = false;
        yield return new WaitForSeconds(_time);
        canMove = true;
    }
}