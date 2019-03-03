﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyButton : ButtonBase
{
    [Header("Enemies to Kill")]
    [SerializeField]
    private List<EnemyBase> enemiesToKillInspector = new List<EnemyBase>();
    private List<IEnemy> enemiesToKill;
    private List<IEnemy> killed;

    #region API
    public override void Init()
    {
        Setup();
    }

    public override void Setup()
    {
        killed = new List<IEnemy>();
        enemiesToKill = new List<IEnemy>();
        EnemyManager.OnEnemyDeath -= HandleOnEnemyDeath; //Workaround orribile
        EnemyManager.OnEnemyDeath += HandleOnEnemyDeath;
        foreach (EnemyBase e in enemiesToKillInspector)
        {
            enemiesToKill.Add(e as IEnemy);
        }
    }

    public override void Activate()
    {
        foreach (DoorBase _current in targets)
        {
            _current.Activate();
        }
    }
    #endregion

    #region Handlers
    private void HandleOnEnemyDeath(IEnemy _enemy)
    {
        if (enemiesToKill.Contains(_enemy) && !killed.Contains(_enemy))
        {
            killed.Add(_enemy);
            Debug.Log("Ucciso " + killed.Count);
        }
        Debug.Log("Ne mancano " + enemiesToKill.Count);
        if (enemiesToKill.Count == killed.Count)
            Activate();
    }
    #endregion

    private void OnDisable()
    {
        EnemyManager.OnEnemyDeath -= HandleOnEnemyDeath;
    }
}
