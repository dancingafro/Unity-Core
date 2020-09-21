using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
#endif
using UnityEngine;

namespace CoreScript.Localisation
{
    [CreateAssetMenu(fileName = "LocalisationInfo", menuName = "Localisation/LocalisationInfo", order = 1)]
    public class LocalisationInfo : ScriptableObject
    {
        [SerializeField] string filePath = "", languagePath = "";
        Languages[] languages = null;

        const string exampleLanguage = "CoreScript/Localisation/Languages";
        const string exampleFile = "CoreScript/Localisation/Example";

        public string FilePath { get { return (filePath == "") ? exampleFile : filePath; } }
        public string LanguagePath { get { return (languagePath == "") ? exampleLanguage : languagePath; } }
        public Languages CurrentLanguage { get; private set; } = null;

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

#if UNITY_EDITOR
        public string CSVFullFilePath
        {
            get
            {
                string CSVFullFilePath = "Packages/Unity Core/Resources/" + FilePath + ".csv";

                if (!File.Exists(CSVFullFilePath))
                {
                    string[] paths = Directory.GetDirectories("Library/PackageCache/");
                    foreach (var path in paths)
                    {
                        if (!path.Contains("Unity Core"))
                            continue;
                        CSVFullFilePath = path + "/Resources/" + FilePath + ".csv";
                        break;
                    }
                }

                return CSVFullFilePath;
            }
        }
#endif
    }
}