using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class LocalisationSystem
    {
        public enum Language
        {
            English,
            French
        }

        public static Language language = Language.English;

        static Dictionary<Language, string> attributePairs = new Dictionary<Language, string>();
        static Dictionary<string, Dictionary<string, string>> localisation;

        public static bool isInit = false;

        static void Init()
        {
            if (isInit)
                return;

            isInit = true;

            attributePairs.Add(Language.English, "en");
            attributePairs.Add(Language.French, "fr");

            CSVLoader csvLoader = new CSVLoader(Resources.Load<TextAsset>("Example").text.Split('\n'));

            localisation = csvLoader.GetDictionaryValues();

        }

        public static string GetLocalisedValue(string key)
        {
            Init();

            string value = "";

            localisation[attributePairs[language]].TryGetValue(key, out value);

            return value;
        }

    }
}