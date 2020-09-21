using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CoreScript.Singleton;

namespace CoreScript.Localisation
{
    public class LocalisationManager : Singleton<LocalisationManager>
    {
        LocalisationData localisationData = null;
        public LocalisationData LocalisationData
        {
            get
            {
                if (localisationData == null)
                    localisationData = LocalisationData.Load();

                return localisationData;
            }
        }

        public string GetLocalisedValue(string key)
        {
            return LocalisationData.GetLocalisedValue(key);
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
#endif
    }
}