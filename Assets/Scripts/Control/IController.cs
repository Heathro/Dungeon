using UnityEngine;
using Attributes;
using Utils;

namespace Control
{
    public interface IController
    {
        Coroutine TakeTurn();

        void EnableControl(bool isEnable);

        void EnableAgent(bool isEnable);

        void EnableObstacle(bool isEnable);

        bool IsEnemy();
        
        Health GetHealth();

        void EndTurn();

        void SetBattleMarker(FighterType fighterType);

        Transform GetTransform();

        float GetInitiative();

        void UpdateBuffs();

        void ApplyBuffEffect();
    }
}