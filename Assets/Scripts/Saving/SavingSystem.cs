using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Control;

namespace Saving
{
    public class SavingSystem : MonoBehaviour
    {
        // PUBLIC

        public IEnumerator LoadLastScene(string file)
        {
            Dictionary<string, object> save = LoadFile(file);

            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (save.ContainsKey("lastSceneBuildIndex"))
            {
                buildIndex = (int)save["lastSceneBuildIndex"];
            }    

            yield return SceneManager.LoadSceneAsync(buildIndex);
            RestoreState(save);
        }

        public void Save(string file)
        {
            Dictionary<string, object> state = LoadFile(file);
            CaptureState(state);
            SaveFile(file, state);
        }

        public void Load(string file)
        {
            RestoreState(LoadFile(file));
        }

        public void Delete(string file)
        {
            File.Delete(GetPathToSaveFile(file));
        }

        public bool SaveFileExist(string file)
        {
            return File.Exists(GetPathToSaveFile(file));
        }

        public IEnumerable<string> GetSaveList()
        {
            foreach (string file in Directory.EnumerateFiles(Application.persistentDataPath))
            {
                if (Path.GetExtension(file) == ".sav")
                {
                    yield return Path.GetFileNameWithoutExtension(file);
                }
            }
        }

        // PRIVATE

        void SaveFile(string file, object state)
        {
            string path = GetPathToSaveFile(file);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }
        
        Dictionary<string, object> LoadFile(string file)
        {
            string path = GetPathToSaveFile(file);
            if (!File.Exists(path))
            {
                return new Dictionary<string, object>();
            }

            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                if (saveable.GetComponent<PlayerController>() != null) continue;

                state[saveable.GetUniqueID()] = saveable.CaptureState();
            }
            foreach (PlayerController playerController in FindObjectOfType<ControlSwitcher>().GetPlayers())
            {
                SaveableEntity saveable = playerController.GetComponent<SaveableEntity>();
                state[saveable.GetUniqueID()] = saveable.CaptureState();
            }
            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                if (saveable.GetComponent<PlayerController>() != null) continue;

                string id = saveable.GetUniqueID();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
            foreach (PlayerController playerController in FindObjectOfType<ControlSwitcher>().GetPlayers())
            {
                SaveableEntity saveable = playerController.GetComponent<SaveableEntity>();

                string id = saveable.GetUniqueID();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }
        
        string GetPathToSaveFile(string file)
        {
            return Path.Combine(Application.persistentDataPath, file + ".sav");
        }
    }
}