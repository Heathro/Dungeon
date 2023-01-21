using UnityEngine;
using Stats;
using Inventories;
using Abilities;

namespace UI.HUD
{
    public class ResistanceDisplay : MonoBehaviour
    {
        // CONFIG

        [SerializeField] ResistanceElement fire = null;
        [SerializeField] ResistanceElement water = null;
        [SerializeField] ResistanceElement earth = null;
        [SerializeField] ResistanceElement air = null;
        [SerializeField] ResistanceElement poison = null;

        // CACHE

        BaseStats baseStats = null;

        // PUBLIC

        public void SetResistanceBar(BaseStats baseStats)
        {
            if (baseStats == null) return;

            this.baseStats = baseStats;

            Equipment equipment = baseStats.GetComponent<Equipment>();
            if (equipment != null) equipment.equipmentUpdated += Redraw;

            baseStats.GetComponent<BuffStore>().buffStoreUpdated += Redraw;

            Redraw();
        }

        // PRIVATE

        void Redraw()
        {
            fire.Setup(baseStats.GetStat(CharacterStat.FireResistance));
            water.Setup(baseStats.GetStat(CharacterStat.WaterResistance));
            earth.Setup(baseStats.GetStat(CharacterStat.EarthResistance));
            air.Setup(baseStats.GetStat(CharacterStat.AirResistance));
            poison.Setup(baseStats.GetStat(CharacterStat.PoisonResistance));
        }
    }
}