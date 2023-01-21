using Control;
using System;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Set Buff Effect", menuName = "Abilities/Effects/Set Buff", order = 0)]
    public class SetBuffEffect : EffectStrategy
    {
        // CONFIG

        [SerializeField] BuffEffect buffEffect = null;

        // PUBLIC

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject target in data.GetTargets())
            {
                target.GetComponent<BuffStore>().StartEffect(buffEffect);

                AIController aiController = target.GetComponent<AIController>();
                if (aiController != null) aiController.Aggrevate();
            }

            finished();
        }

        public override void StartEffect(AbilityData data, GameObject target)
        {
            BuffStore buffStore = target.GetComponent<BuffStore>();
            if (buffStore == null) return;

            AIController aiController = target.GetComponent<AIController>();
            if (aiController != null) aiController.Aggrevate();

            buffStore.StartEffect(buffEffect);
        }
    }
}