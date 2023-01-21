using System;
using UnityEngine;
using Control;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Add Points", menuName = "Abilities/Effects/Add Points", order = 0)]
    public class AddPointsEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] int points = 0;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.GetUser().GetComponent<PlayerController>().AddActionPoints(points);
            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            PlayerController playerController = target.GetComponent<PlayerController>();
            if (playerController == null) return;
            
            playerController.AddActionPoints(points);
        }
    }
}