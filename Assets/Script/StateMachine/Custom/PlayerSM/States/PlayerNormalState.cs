﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.PlayerSM
{
    public class PlayerNormalState : PlayerSMStateBase
    {
        public override void Enter()
        {
            context.player.OnEnemyCollision += OnEnemyCollision;            
            context.player.GetShotController().ChangeShotType();
            context.player.GetMovementController().SetCanMove(true);
        }

        public override void Exit()
        {
            context.player.OnEnemyCollision -= OnEnemyCollision;
        }

        public override void Tick()
        {
            if (Input.GetButtonDown("Parasite") || CheckJoystickLTAxis())
            {
                IEnemy e = context.player.GetParasiteController().CheckParasite();
                if (e != null)
                {
                    context.player.StartParasiteCoroutine(e);
                    context.player.GetMovementController().SetCanMove(false);
                }
                else
                    Debug.Log("Non ci sono nemici stunnati nelle vicinanze");
            }

            if (_playerImmunity)
            {
                if (immunityTime <= 0)
                    PlayerImmunityEnd();

                immunityTime -= Time.deltaTime;
            }

            context.player.GetHealthController().LoseHealthOvertime();
        }

        bool _playerImmunity;
        float immunityTime;
        private void OnEnemyCollision(IEnemy _enemy)
        {
            context.player.OnEnemyCollision -= OnEnemyCollision;
            context.player.GetHealthController().LoseHealth(_enemy.GetDamage());
            context.player.GetCollisionController().CheckDamageCollision(false);
            context.player.gameObject.layer = LayerMask.NameToLayer("PlayerImmunity");
            
            immunityTime = context.player.GetCollisionController().GetImmunityDuration();
            _playerImmunity = true;
        }

        private void PlayerImmunityEnd()
        {
            _playerImmunity = false;
            context.player.OnEnemyCollision += OnEnemyCollision;
            context.player.GetCollisionController().CheckDamageCollision(true);
            context.player.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
}
