using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [System.Serializable]
    public class Condition
    {
        // CONFIG

        [SerializeField] Disjunction[] and;

        // PUBLIC


        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            foreach (Disjunction disj in and)
            {
                if (!disj.Check(evaluators))
                {
                    return false;
                }
            }
            return true;
        }

        [System.Serializable]
        class Disjunction
        {
            // CONFIG

            [SerializeField] Predicate[] or;

            // PUBLIC

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                foreach (Predicate pred in or)
                {
                    if (pred.Check(evaluators))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [System.Serializable]
        class Predicate
        {
            // CONFIG

            [SerializeField] string predicate;
            [SerializeField] string[] parameters;
            [SerializeField] bool negate = false;
        
            // PUBLIC

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                foreach (IPredicateEvaluator evaluator in evaluators)
                {
                    bool? result = evaluator.Evaluate(predicate, parameters);
                    if (result == null) continue;
                    if (result == negate) return false;
                }
                return true;
            }
        }        
    }
}