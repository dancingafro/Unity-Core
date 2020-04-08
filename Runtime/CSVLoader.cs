using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CoreScript.Localisation
{
    public class CSVLoader
    {
        TextAsset csvFile;
        char surround = '"';
        string[] fieldSeperator = { "\",\"" };

        public CSVLoader()
        {
            LoadCSV();
        }

        public void LoadCSV()
        {
            csvFile = Resources.Load<TextAsset>("Example");
        }

        public Dictionary<string, Dictionary<string, string>> GetDictionary()
        {
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

            string[] lines = csvFile.text.Split('\n');

            string[] headers = lines[0].Split(fieldSeperator, StringSplitOptions.None);

            for (int i = 1; i < headers.Length; i++)
                dictionary.Add(headers[i], new Dictionary<string, string>());

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for (int i = 0; i < lines.Length; i++)
            {
                string[] fields = CSVParser.Split(lines[i]);
                for (int x = 0; x < fields.Length; x++)
                {
                    fields[x] = fields[x].TrimStart(' ', surround);
                    fields[x] = fields[x].TrimEnd(surround);
                }

                if (fields.Length < dictionary.Count || dictionary.ContainsKey(fields[0]))
                    continue;

                int index = 1;
                foreach (var Key in dictionary.Keys)
                {
                    dictionary[Key].Add(fields[0], fields[index]);
                    ++index;
                }

            }

            return dictionary;
        }
#if UNITY_EDITOR
        public void Add(string key, string value)
        {
            string append = string.Format("\n\"{0}\",\"{1}\",\"\"", key, value);
            File.AppendAllText("Assets/Resources/Example.csv", append);

            UnityEditor.AssetDatabase.Refresh();
        }

        public void Remove(string key)
        {
            string[] lines = csvFile.text.Split('\n'), keys = new string[lines.Length];


            for (int i = 0; i < keys.Length; i++)
                keys[i] = lines[i].Split(fieldSeperator, StringSplitOptions.None)[0];

            int index = -1;

            for (int i = 0; i < keys.Length; i++)
            {
                if (!keys[i].Contains(key))
                    continue;

                index = i;
                break;
            }

            if (index == -1)
                return;

            string[] newLines = lines.Where(i => i != lines[index]).ToArray();

            File.WriteAllText("Assets/Resources/Example.csv", string.Join("\n", newLines));

            UnityEditor.AssetDatabase.Refresh();
        }

        public void Edit(string key, string value)
        {
            Remove(key);
            Add(key, value);
        }
#endif
    }
}