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
        [SerializeField] Languages[] languages = null;
        [SerializeField] TextAsset[] CSVFiles = null;

        [SerializeField] CSVData[] csvDatas;

        [SerializeField] string csvPath = "CoreScript/Localisation/Example", languagePath = "CoreScript/Localisation/Languages";
        public string CSVPath { get { return csvPath; } }
        public string LanguagePath { get { return languagePath; } }
        public Languages CurrentLanguage { get; private set; } = null;

        Dictionary<string, Dictionary<string, string>> localisation = null;
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
            if (string.IsNullOrEmpty(key) || !Localisation.ContainsKey(CurrentLanguage.Header) || !Localisation[CurrentLanguage.Header].ContainsKey(key))
                return "";

            return Localisation[CurrentLanguage.Header][key];
        }

        public void LoadLocalisation(bool reset = false)
        {
            localisation = null;
            localisation = new Dictionary<string, Dictionary<string, string>>();
            //TODO
            CSVFiles = Resources.LoadAll<TextAsset>(CSVPath);

            csvDatas = new CSVData[CSVFiles.Length];

            for (int i = 0; i < CSVFiles.Length; i++)
            {
                csvDatas[i] = CSVLoader.LoadCSV(CSVFiles[i]);
                localisation.Add(csvDatas[i].headers[0], csvDatas[i].data);
            }

            for (int i = 0; i < CSVFiles.Length; i++)
            {
                bool hit = false;
                int index = -1;
                for (int a = 0; a < Languages.Length; a++)
                {
                    if (CSVFiles[i].name != Languages[a].name)
                        continue;

                    hit = true;
                    index = a;
                    break;
                }
                if (hit)
                {
                    Languages[index].Header = csvDatas[i].headers[0];
                    Languages[index].DefaultLanguage = bool.Parse(csvDatas[i].headers[1]);
                }
#if UNITY_EDITOR
                else
                {
                    string additionalPath = Application.dataPath + "/Resources/" + LanguagePath;
                    if (!Directory.Exists(additionalPath))
                        Directory.CreateDirectory(additionalPath);

                    Languages language = CreateInstance<Languages>();
                    language.Header = csvDatas[i].headers[0];
                    language.DefaultLanguage = bool.Parse(csvDatas[i].headers[1]);

                    UnityEditor.AssetDatabase.CreateAsset(language, "Assets/Resources/" + LanguagePath + "/" + CSVFiles[i].name + ".asset");
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                }
#endif
            }

            if (reset || !CurrentLanguage)
                UpdateData();
        }

        public Dictionary<string, string> GetDictionary(string header = "")
        {
            if (header == "")
                header = CurrentLanguage.Header;

            return Localisation[header];
        }

#if UNITY_EDITOR
        public void Add(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Add(key, value, localisationInfo.CurrentLanguage.Header);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public void Replace(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Edit(key, value, localisationInfo.CurrentLanguage.Header);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public void Remove(string key)
        {
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Remove(key);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public void Add(string lang, string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Add(key, value, localisationInfo.CurrentLanguage.Header);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public void Replace(string lang, string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Edit(key, value, localisationInfo.CurrentLanguage.Header);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public void Remove(string lang, string key)
        {
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);
            //CSVLoader.Remove(key);
            //CSVLoader.LoadCSV(LocalisationData.CSVPath);

            //UpdateDictionary();
        }

        public string CSVFullFilePath
        {
            get
            {
                string CSVFullFolderPath = "Assets/Resources/" + CSVPath + ".csv";
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
            LocalisationData localisationData = Resources.Load<LocalisationData>(path + "/LocalisationData");
            if (localisationData != null)
                return localisationData;
#if UNITY_EDITOR
            string additionalPath = Application.dataPath + "/Resources/" + path;
            localisationData = CreateInstance<LocalisationData>();

            if (!Directory.Exists(additionalPath))
                Directory.CreateDirectory(additionalPath);

            UnityEditor.AssetDatabase.CreateAsset(localisationData, "Assets/Resources/" + path + "/LocalisationData.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.LogWarning("Could not find LocalisationData asset. Will use default settings instead.");
            return localisationData;
#else
            return null;
#endif
        }

    }
}