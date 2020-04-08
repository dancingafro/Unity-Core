using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoreScript.Localisation
{
    public static class LocalisationSystem
    {
        public static Languages currentLanguage = null;

        static Languages[] languages = null;
        static Dictionary<string, Dictionary<string, string>> localisation;

        public static bool isInit = false;
        static CSVLoader csvLoader;

        public static CSVLoader CSVLoader
        {
            get
            {
                if (csvLoader == null)
                    csvLoader = new CSVLoader();

                return csvLoader;
            }
        }

        public static Dictionary<string, string> GetDictionary()
        {
            Init();

            return localisation[currentLanguage.Key];
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

            localisation[currentLanguage.Key].TryGetValue(key, out string value);

            return value;
        }

        public static void RefreshData()
        {
            CSVLoader.LoadCSV();
            UpdateDictionary();
        }

        static void UpdateDictionary()
        {
            languages = Resources.LoadAll<Languages>("Languages");

            foreach (var item in languages)
            {
                if (!item.DefaultLanguage)
                    continue;

                currentLanguage = item;
                break;
            }

            localisation = CSVLoader.GetDictionary();
        }

        public static void Add(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            CSVLoader.LoadCSV();
            CSVLoader.Add(key, value);
            CSVLoader.LoadCSV();

            UpdateDictionary();
        }

        public static void Replace(string key, string value)
        {
            if (value.Contains("\""))
                value.Replace('"', '\"');

            CSVLoader.LoadCSV();
            CSVLoader.Edit(key, value);
            CSVLoader.LoadCSV();

            UpdateDictionary();
        }

        public static void Remove(string key)
        {
            CSVLoader.LoadCSV();
            CSVLoader.Remove(key);
            CSVLoader.LoadCSV();

            UpdateDictionary();
        }
    }
}