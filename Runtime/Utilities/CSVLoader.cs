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
            string[] lines = csv.text.Split('\n'), headers = TrimAndSplit(lines[0]), keys = new string[lines.Length - 1];

            for (int i = 0; i < keys.Length; ++i)
                keys[i] = lines[i + 1].Split(fieldSeperator, StringSplitOptions.None)[0];

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

        //#if UNITY_EDITOR
        //        public static void Add(string key, string value, string header)
        //        {
        //            string append = string.Format("\n\"{0}\",{1}", key, AppendToCSV(value, header));
        //            //File.AppendAllText(localisationInfo.CSVFullFilePath, append);

        //            UnityEditor.AssetDatabase.Refresh();
        //        }

        //        static string AppendToCSV(string value, string header)
        //        {
        //            string line = "";
        //            string[] headers = Headers;

        //            for (int i = 1; i < headers.Length; ++i)
        //                line += (line != "" ? "," : "") + "\"" + (headers[i] == header ? value : "") + "\"";

        //            return line;
        //        }

        //        public static void Remove(string key)
        //        {
        //            string[] lines = Lines, keys = Keys;

        //            int lineIndexToRemove = -1;

        //            for (int i = 0; i < keys.Length; ++i)
        //            {
        //                if (!keys[i].Contains(key))
        //                    continue;

        //                lineIndexToRemove = i;
        //                break;
        //            }

        //            if (lineIndexToRemove == -1)
        //                return;

        //            string[] newLines = lines.Where(i => i != lines[lineIndexToRemove]).ToArray();

        //            //File.WriteAllText(localisationInfo.CSVFullFilePath, string.Join("\n", newLines));

        //            UnityEditor.AssetDatabase.Refresh();
        //        }

        //        public static void Edit(string key, string value)
        //        {
        //            string[] lines = Lines, keys = Keys;

        //            int lineIndexToEdit = -1;

        //            for (int i = 0; i < keys.Length; ++i)
        //            {
        //                if (!keys[i].Contains(key))
        //                    continue;

        //                lineIndexToEdit = i;
        //                break;
        //            }

        //            if (lineIndexToEdit == -1)
        //                return;

        //            string[] fields = TrimAndSplit(lines[lineIndexToEdit]);

        //            fields[1] = value;
        //            lines[lineIndexToEdit] = JoinFields(fields);

        //            //File.WriteAllText(localisationInfo.CSVFullFilePath, string.Join("\n", lines));

        //            UnityEditor.AssetDatabase.Refresh();
        //        }
        //#endif
    }
}