﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenManager : MonoBehaviour
{
    #region Delegates
    public delegate void FinishTokenEvent();
    public FinishTokenEvent FinishToken;
    #endregion

    [Header("Token Container")]
    [SerializeField]
    private Transform tokenContainer;

    private List<BaseToken> tokens = new List<BaseToken>();

    [Header("Debug")]
    [SerializeField]
    private int tokenCounter;

    #region API
    public void Init()
    {
        if (tokenContainer != null)
        {
            for (int i = 0; i < tokenContainer.childCount; i++)
            {
                BaseToken _current = tokenContainer.GetChild(i).GetComponent<BaseToken>();
                _current.Init();
                tokens.Add(_current);
                _current.GetToken += HandleGetToken;
            }
        }

        tokenCounter = 0;
    }
    #endregion

    #region Handlers
    private void HandleGetToken(BaseToken _token)
    {
        tokenCounter++;
        if (tokenCounter == tokens.Count)
        {
            if (FinishToken != null)
                FinishToken();
        }
    }
    #endregion

}
