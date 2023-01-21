using System;
using System.Collections.Generic;
using UnityEngine;
using Saving;
using Stats;

namespace Control
{
    public class Explorer : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] float corridor = 100f;
        [SerializeField] float lab = 200f;
        [SerializeField] float kitchen = 250f;
        [SerializeField] float crypt = 350f;
        [SerializeField] float shrine = 300f;
        [SerializeField] float main = 150f;
        [SerializeField] float boss = 200f;

        // CACHE

        ControlSwitcher controlSwitcher = null;

        // STATE

        List<int> exploredScenes = new List<int>();
        Dictionary<int, float> amounts = new Dictionary<int, float>();
        
        // LIFECYCLE

        void Awake()
        {
            controlSwitcher = GetComponent<ControlSwitcher>();

            amounts[2] = corridor;
            amounts[3] = lab;
            amounts[4] = kitchen;
            amounts[5] = crypt;
            amounts[6] = shrine;
            amounts[7] = main;
            amounts[8] = boss;
        }

        // PUBLIC

        public bool GainExperience(int sceneNumber)
        {
            if (sceneNumber < 2) return false;
            if (exploredScenes.Contains(sceneNumber)) return false;

            exploredScenes.Add(sceneNumber);
            
            foreach (PlayerController player in controlSwitcher.GetPlayers())
            {
                player.GetComponent<Experience>().GainExperience(amounts[sceneNumber]);
            }

            return true;
        }

        // PRIVATE

        object ISaveable.CaptureState()
        {
            return exploredScenes;
        }

        void ISaveable.RestoreState(object state)
        {
            exploredScenes = (List<int>)state;
        }
    }
}