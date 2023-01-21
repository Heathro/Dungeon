using UnityEngine;
using Utils;

namespace UI.Ingame
{
    public class BattleMarker : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Material control;
        [SerializeField] Material companion;
        [SerializeField] Material enemy;

        // CACHE

        MeshRenderer meshRenderer;

        // LIFECYCLE

        void Awake()
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        void Start()
        {
            meshRenderer.enabled = false;
        }

        // PUBLIC

        public void SetBattleMarker(FighterType fighterType)
        {
            meshRenderer.enabled = fighterType != FighterType.None;

            switch (fighterType)
            {
                case FighterType.Control: meshRenderer.material = control; break;
                case FighterType.Companion: meshRenderer.material = companion; break;
                case FighterType.Enemy: meshRenderer.material = enemy; break;
            }
        }
    }
}