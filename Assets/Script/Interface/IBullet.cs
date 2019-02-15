﻿using UnityEngine;

/// <summary>
/// Interfaccia implementata sui proiettili
/// </summary>
public interface IBullet
{
    void Shot(int _damage, float _speed, float _range, Vector3 _shotPosition, Vector3 _direction);
    void Shot(int _damage, float _speed, float _range, Vector3 _shotPosition, Transform _target);

    /// <summary>
    /// Funzione che ritrona il danno che fa il proiettile
    /// </summary>
    /// <returns></returns>
    int GetBulletDamage();
}
