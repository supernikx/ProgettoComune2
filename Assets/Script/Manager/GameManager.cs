﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.GameSM;
using UI;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameManager instance;
    /// <summary>
    /// Riferimento alla GameSM
    /// </summary>
    GameSMController gameSM;
    /// <summary>
    /// Riferimento all'ui manager
    /// </summary>
    UI_ManagerBase uiManager;

    private void Awake()
    {
        //Get Components
        gameSM = GetComponent<GameSMController>();

        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            DestroyImmediate(gameObject);
    }

    void Start()
    {
        gameSM.Init(this);
    }

    #region API
    /// <summary>
    /// Funzione che cerca un ui manager in scena e se è diverso da quello precedente lo sostituisce
    /// </summary>
    public UI_ManagerBase FindUIManager()
    {
        UI_ManagerBase newUi = FindObjectOfType<UI_ManagerBase>();
        if (newUi != uiManager)
        {
            uiManager = newUi;
            uiManager.Setup(this);
        }
        
        return uiManager;
    }

    /// <summary>
    /// Funzione che ritorna l'ui manager
    /// </summary>
    public UI_ManagerBase GetUIManager()
    {
        return uiManager;
    }

    /// <summary>
    /// Funzione che inizia la partita
    /// </summary>
    public static void StartGame()
    {
        if (instance.gameSM.GoToLevelSetup != null)
            instance.gameSM.GoToLevelSetup();
    }

    /// <summary>
    /// FUnzione che ricarica il livello attuale
    /// </summary>
    public static void RestartCurrentLevel()
    {
        if (instance.gameSM.GoToLevelSetup != null)
            instance.gameSM.GoToLevelSetup();
    }

    /// <summary>
    /// Funzione che chiude l'applicazione
    /// </summary>
    public static void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
