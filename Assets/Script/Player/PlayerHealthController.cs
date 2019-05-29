﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHealthController : MonoBehaviour
{
    #region Delegates
    public delegate void PlayerHealthDelegates(float health);
    public static PlayerHealthDelegates OnHealthChange;
    #endregion

    [Header("Health Settings")]
    [SerializeField]
    [Tooltip("The time the energy will take to drop from max to 0")]
    private float timeToDeplete;
    [SerializeField]
    [Tooltip("The time the energy will take to increase from 0 to max")]
    private float timeToFill;
    [SerializeField]
    [Tooltip("Max Health")]
    private float maxHealth = 100;
    [SerializeField]
    [Tooltip("Min Health")]
    private float minHealth = 0;

    /// <summary>
    /// Player Health
    /// </summary>
    private float health
    {
        set
        {
            _health = value;
            if (OnHealthChange != null)
                OnHealthChange(_health);
        }
        get
        {
            return _health;
        }
    }
    private float _health;

    /// <summary>
    /// Amount of health lost every frame
    /// </summary>
    private float lossPerSecond;

    /// <summary>
    /// Amount of health gained every frame
    /// </summary>
    private float gainPerSecond;

    /// <summary>
    /// Reference to Player
    /// </summary>
    private Player player;

    #region API
    /// <summary>
    /// Initialize this script
    /// </summary>
    public void Init(Player _player)
    {
        if (timeToDeplete == -1)
            lossPerSecond = -1;
        else
            lossPerSecond = (maxHealth - minHealth) / timeToDeplete;

        gainPerSecond = (maxHealth - minHealth) / timeToFill;
        player = _player;

        Setup();
    }

    public void Setup()
    {
        health = maxHealth;
    }

    /// <summary>
    /// Lose health every update
    /// </summary>
    public void LoseHealthOverTime()
    {
        if (lossPerSecond == -1)
            return;

        health = Mathf.Clamp(health - lossPerSecond * Time.deltaTime, minHealth, maxHealth);
        if (health == minHealth)
            player.StartDeathCoroutine();
    }

    /// <summary>
    /// Funzione che diminuisce la vita del valore passato come parametro
    /// </summary>
    /// <param name="_health"></param>
    public void DamageHit(float _health, float _time = 0)
    {
        if (_time == 0)
        {
            health = Mathf.Clamp(health - _health, minHealth, maxHealth);
            if (health == minHealth)
                player.StartDeathCoroutine();
        }
        else
        {
            StartCoroutine(DamageHitOverTime(_health, _time));
        }
    }
    /// <summary>
    /// Coroutine che fa perdere vita overtime al player
    /// </summary>
    /// <param name="_health"></param>
    /// <param name="_time"></param>
    /// <returns></returns>
    private IEnumerator DamageHitOverTime(float _health, float _time)
    {
        float tickDuration = 0.5f;
        float damgeEachTick = tickDuration * _health / _time;
        int ticks = Mathf.RoundToInt(_time / tickDuration);
        int tickCounter = 0;
        while (tickCounter < ticks)
        {
            health = Mathf.Clamp(health - damgeEachTick, minHealth, maxHealth);
            tickCounter++;
            yield return new WaitForSeconds(tickDuration);
        }
    }

    /// <summary>
    /// gain health every update
    /// </summary>
    public bool GainHealthOverTime()
    {
        health = Mathf.Clamp(health + gainPerSecond * Time.deltaTime, minHealth, maxHealth);
        if (health == maxHealth)
        {
            return true;
        }
        return false;
    }

    #region Getter
    /// <summary>
    /// Funzione che ritorna la vita attuale
    /// </summary>
    /// <returns></returns>
    public float GetHealth()
    {
        return health;
    }
    #endregion
    #endregion

}
