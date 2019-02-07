﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager singleton;
    /// <summary>
    /// Referenza al Pool manager
    /// </summary>
    PoolManager poolMng;
    /// <summary>
    /// Referenza al player
    /// </summary>
    Player player;
    /// <summary>
    /// Referenza all'Enemy Manager
    /// </summary>
    EnemyManager enemyMng;
    /// <summary>
    /// Reference to Platform Manager
    /// </summary>
    private PlatformManager platformMng;
    /// <summary>
    /// Reference to Doors and Buttons Manager
    /// </summary>
    private DoorsButtonsManager doorsButtonsMng;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    #region API
    /// <summary>
    /// Inizializzazione elementi del livello (verrà chiamata dalla SM)
    /// </summary>
    public void Init()
    {
        singleton = this;

        //Init
        poolMng = GetComponent<PoolManager>();
        if (poolMng != null)
            poolMng.Init();

        enemyMng = GetComponent<EnemyManager>();
        if (enemyMng != null)
            enemyMng.Init();

        platformMng = GetComponent<PlatformManager>();
        if (platformMng != null)
            platformMng.Init();

        doorsButtonsMng = GetComponent<DoorsButtonsManager>();
        if (doorsButtonsMng != null)
            doorsButtonsMng.Init();

        player = FindObjectOfType<Player>();
        if (player != null)
            player.Init(enemyMng, platformMng);

        //Setup
        enemyMng.EnemiesSetup();

        //Iscrizione Eventi
        player.OnPlayerDeath += HandlePlayerDeath;
    }
    #endregion

    #region Handlers
    private void HandlePlayerDeath()
    {
        Debug.Log("The Player is dead");
    }
    #endregion

    #region Getter
    public EnemyManager GetEnemyManager()
    {
        return enemyMng;
    }
    #endregion
}
