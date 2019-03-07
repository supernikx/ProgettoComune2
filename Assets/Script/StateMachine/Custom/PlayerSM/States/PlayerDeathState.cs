﻿using UnityEngine;
using System.Collections;
using StateMachine.PlayerSM;
using System;

public class PlayerDeathState : PlayerSMStateBase
{

    public override void Enter()
    {
        LevelManager.OnGameOver += HandleGameover;
        PlayerVFXController.OnDeathVFXEnd += HandleDeathVFXEnd;
        gameover = false;
        context.player.GetLivesController().LoseLives();
        context.player.ChangeGraphics(context.player.GetPlayerGraphic());
        context.player.GetAnimatorController().SetAnimator(context.player.GetAnimatorController().GetPlayerAnimator());
        context.player.GetShotController().ResetEnemyShot();
        context.player.GetActualGraphic().transform.localScale = new Vector3(1, 1, 1);
        context.player.GetActualGraphic().SetActive(false);
        context.player.GetCollisionController().OnStickyEnd();
        context.player.GetCollisionController().GetPlayerCollider().enabled = false;
        context.player.GetCollisionController().GetCollisionInfo().ResetAll();
        context.player.GetMovementController().SetCanMove(false);
        context.player.GetShotController().SetCanShoot(false);
    }

    bool gameover;
    private void HandleGameover()
    {
        gameover = true;
    }

    private void HandleDeathVFXEnd()
    {
        context.player.GoToNormalState();
    }

    public override void Exit()
    {
        LevelManager.OnGameOver -= HandleGameover;
        PlayerVFXController.OnDeathVFXEnd -= HandleDeathVFXEnd;

        context.player.GetCollisionController().GetPlayerCollider().enabled = true;
        if (gameover)
            context.player.GetLivesController().Init();
        context.player.GetHealthController().Setup();
        context.player.GetActualGraphic().SetActive(true);
        context.player.GetMovementController().SetCanMove(true);
        context.player.GetShotController().SetCanShoot(true);
        context.player.transform.position = context.checkpointManager.GetActiveCheckpoint().GetPosition();
    }

}
