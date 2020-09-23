using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CoreScript.Localisation
{
    public struct CSVData
    {
        public string[] headers;
        public Dictionary<string, string> data;
    }

    public static class CSVLoader
    {
        const char surround = '"';
        static readonly string[] fieldSeperator = { "\",\"" };

        //string[] Lines { get { return csvFile.text.Split('\n'); } }
        //string[] Headers { get { return TrimAndSplit(Lines[0]); } }
        //string[] Keys
        //{
        //    get
        //    {
        //        string[] lines = Lines, keys = new string[lines.Length];

        //        for (int i = 0; i < keys.Length; ++i)
        //            keys[i] = lines[i].Split(fieldSeperator, StringSplitOptions.None)[0];

        //        return keys;
        //    }
        //}
        public static CSVData LoadCSV(TextAsset csv)
        {
            string[] lines = csv.text.Split('\n'), headers = TrimAndSplit(lines[0]);

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int i = 1; i < lines.Length; ++i)
            {
                string[] fields = TrimAndSplit(lines[i]);

                if (dictionary.ContainsKey(fields[0]))
                    continue;

                dictionary.Add(fields[0], fields[1]);
            }

            return new CSVData()
            {
                headers = headers,
                data = dictionary
            };
        }

        static string[] ExtractKeys(string[] lines)
        {
            string[] keys = new string[lines.Length - 1];

            for (int i = 0; i < keys.Length; ++i)
            {
                keys[i] = SeperateFields(lines[i + 1])[0];
                keys[i] = keys[i].Replace("\"", "");
            }
            return keys;
        }

        static string[] SeperateFields(string line)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            return CSVParser.Split(line);
        }

        static string[] TrimAndSplit(string line)
        {
            string[] fields = SeperateFields(line);

            for (int i = 0; i < fields.Length; ++i)
            {
                fields[i] = fields[i].TrimStart(' ', surround);
                fields[i] = fields[i].TrimEnd('\r', surround);
            }

            return fields;
        }

        static string JoinFields(string[] fields)
        {
            return surround + string.Join(fieldSeperator[0], fields) + surround;
        }

#if UNITY_EDITOR
        public static void Add(string key, string value, TextAsset csv)
        {
            File.AppendAllText(UnityEditor.AssetDatabase.GetAssetPath(csv), string.Format("\n\"{0}\",{1}", key, "\"" + value + "\""));
            UnityEditor.AssetDatabase.Refresh();
        }

        public static void Remove(string key, TextAsset csv)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(csv);
            string[] lines = csv.text.Split('\n'), keys = ExtractKeys(lines);

            if (!GetIndexFromText(keys, key, out int lineIndexToRemove))
                return;

            string[] newLines = lines.Where(i => i != lines[lineIndexToRemove]).ToArray();

            File.WriteAllText(path, string.Join("\n", newLines));

            UnityEditor.AssetDatabase.Refresh();
        }

        public static void Edit(string key, string value, TextAsset csv)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(csv);

            string[] lines = csv.text.Split('\n'), keys = ExtractKeys(lines);

            if (!GetIndexFromText(keys, key, out int lineIndexToEdit))
                return;

            string[] fields = TrimAndSplit(lines[lineIndexToEdit]);

            fields[1] = value;
            lines[lineIndexToEdit] = JoinFields(fields);

            File.WriteAllText(path, string.Join("\n", lines));

            UnityEditor.AssetDatabase.Refresh();
        }

        static bool GetIndexFromText(string[] keys, string key, out int lineIndex)
        {
            lineIndex = -1;

            for (int i = 0; i < keys.Length; ++i)
            {
                if (keys[i] != key)
                    continue;

                lineIndex = i + 1;
                return true;
            }

            return false;
        }
#endif
    }
}