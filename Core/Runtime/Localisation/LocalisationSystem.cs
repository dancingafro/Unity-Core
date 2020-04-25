using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CoreScript.Singleton;

namespace CoreScript.Localisation
{
    public static class LocalisationSystem
    {
        static LocalisationInfo localisationInfo;

        static Dictionary<string, Dictionary<string, string>> localisation;

        public static bool isInit = false;
        static CSVLoader csvLoader;

        static LocalisationInfo LocalisationInfo
        {
            get
            {
                if (localisationInfo == null)
                    localisationInfo = Resources.Load<LocalisationInfo>("CoreScript/Localisation/LocalisationInfo");

                return localisationInfo;
            }
        }

        public static CSVLoader CSVLoader
        {
            get
            {
                if (csvLoader == null)
                    csvLoader = new CSVLoader(LocalisationInfo);

                return csvLoader;
            }
        }

        public static Dictionary<string, string> GetDictionary()
        {
            Init();

            return localisation[LocalisationInfo.CurrentLanguage.Header];
        }

        static void Init()
        {
            if (isInit)
                return;

            isInit = true;

            UpdateDictionary();
        }

        public static string GetLocalisedValue(string key)
        {
            Init();
            localisation[LocalisationInfo.CurrentLanguage.Header].TryGetValue(key, out string value);
            return value ?? "";
        }

        public static void RefreshData()
        {
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);
            UpdateDictionary();
        }

        static void UpdateDictionary()
        {
            LocalisationInfo.UpdateData();
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);
            localisation = CSVLoader.GetDictionary();
        }

#if UNITY_EDITOR
        public static void Add(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            CSVLoader.LoadCSV(LocalisationInfo.FilePath);
            CSVLoader.Add(key, value, localisationInfo.CurrentLanguage.Header);
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);

            UpdateDictionary();
        }

        public static void Replace(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            CSVLoader.LoadCSV(LocalisationInfo.FilePath);
            CSVLoader.Edit(key, value, localisationInfo.CurrentLanguage.Header);
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);

            UpdateDictionary();
        }

        public static void Remove(string key)
        {
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);
            CSVLoader.Remove(key);
            CSVLoader.LoadCSV(LocalisationInfo.FilePath);

            UpdateDictionary();
        }
#endif
    }
}