using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
#endif
using UnityEngine;

namespace CoreScript.Localisation
{
    public class LocalisationData : ScriptableObject
    {
        [SerializeField] string csvPath = "", languagePath = "";
        Languages[] languages = null;

        const string exampleLanguage = "CoreScript/Localisation/Languages";
        const string exampleCSV = "CoreScript/Localisation/Example";

        public string CSVPath { get { return (csvPath == "") ? exampleCSV : csvPath; } }
        public string LanguagePath { get { return (languagePath == "") ? exampleLanguage : languagePath; } }
        public Languages CurrentLanguage { get; private set; } = null;

        Dictionary<string, Dictionary<string, string>> localisation;
        public Dictionary<string, Dictionary<string, string>> Localisation
        {
            get
            {
                if (localisation == null)
                    LoadLocalisation();

                return localisation;
            }
        }

        public Languages[] Languages
        {
            get
            {
                if (languages == null)
                    UpdateData();

                return languages;
            }
        }

        public void UpdateData()
        {
            languages = Resources.LoadAll<Languages>(LanguagePath);

            CurrentLanguage = null;
            for (int i = 0; i < languages.Length; i++)
            {
                if (!languages[i].DefaultLanguage)
                    continue;

                CurrentLanguage = languages[i];
            }
        }

        public string GetLocalisedValue(string key)
        {
            return Localisation[CurrentLanguage.Header][key];
        }

        public void LoadLocalisation(bool reset = false)
        {
            if (reset || !CurrentLanguage)
            {
                //TODO
                //CurrentLanguage = value;
            }
        }

#if UNITY_EDITOR
        public string CSVFullFilePath
        {
            get
            {
                string CSVFullFolderPath = "Packages/Unity Core/Resources/" + CSVPath + ".csv";

                if (!File.Exists(CSVFullFolderPath))
                {
                    string[] paths = Directory.GetDirectories("Library/PackageCache/");
                    foreach (var path in paths)
                    {
                        if (!path.Contains("Unity Core"))
                            continue;
                        CSVFullFolderPath = path + "/Resources/" + CSVPath + ".csv";
                        break;
                    }
                }

                return CSVFullFolderPath;
            }
        }
#endif

        public static LocalisationData Load()
        {
            string path = "CoreScript/Localisation";
            LocalisationData cursorManagerData = Resources.Load<LocalisationData>(path + "/LocalisationData");
            if (cursorManagerData != null)
                return cursorManagerData;
#if UNITY_EDITOR
            string aditionalPath = "Assets/Resources/" + path;
            cursorManagerData = CreateInstance<LocalisationData>();
            if (!UnityEditor.AssetDatabase.IsValidFolder(aditionalPath))
            {
                string[] paths = aditionalPath.Split('/');
                for (int i = 1; i < paths.Length; i++)
                {
                    if (UnityEditor.AssetDatabase.IsValidFolder(paths[i]))
                        continue;

                    UnityEditor.AssetDatabase.CreateFolder(paths[i - 1], paths[i]);
                }
            }
            UnityEditor.AssetDatabase.CreateAsset(cursorManagerData, aditionalPath + "LocalisationData.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.LogWarning("Could not find LocalisationData asset. Will use default settings instead.");
            return cursorManagerData;
#else
            return null;
#endif
        }

    }
}