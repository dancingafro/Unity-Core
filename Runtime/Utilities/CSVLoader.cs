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
        LocalisationInfo localisationInfo;

        TextAsset csvFile;

        TextAsset CSVFile
        {
            get
            {
                if (csvFile == null)
                    LoadCSV(localisationInfo.FilePath);

                return csvFile;
            }
        }

        char surround = '"';
        string[] fieldSeperator = { "\",\"" };

        string[] Lines { get { return CSVFile.text.Split('\n'); } }
        string[] Headers { get { return TrimAndSplit(Lines[0]); } }
        string[] Keys
        {
            get
            {
                string[] lines = Lines, keys = new string[lines.Length];

                for (int i = 0; i < keys.Length; ++i)
                    keys[i] = lines[i].Split(fieldSeperator, StringSplitOptions.None)[0];

                return keys;
            }
        }

        public CSVLoader(LocalisationInfo localisationInfo)
        {
            this.localisationInfo = localisationInfo;
            LoadCSV(localisationInfo.FilePath);
        }

        public void LoadCSV(string filePath)
        {
            csvFile = Resources.Load<TextAsset>(filePath);
        }

        public Dictionary<string, Dictionary<string, string>> GetDictionary()
        {
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

            string[] lines = Lines, keys = Keys, headers = Headers;

            for (int i = 1; i < headers.Length; ++i)
                dictionary.Add(headers[i], new Dictionary<string, string>());

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] fields = TrimAndSplit(lines[i]);

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

        string[] SeperateFields(string line)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            return CSVParser.Split(line);
        }

        string[] TrimAndSplit(string line)
        {
            string[] fields = SeperateFields(line);

            for (int i = 0; i < fields.Length; ++i)
            {
                fields[i] = fields[i].TrimStart(' ', surround);
                fields[i] = fields[i].TrimEnd('\r', surround);
            }

            return fields;
        }

        string JoinFields(string[] fields)
        {
            return surround + string.Join(fieldSeperator[0], fields) + surround;
        }

        int IndexOfHeader(string header)
        {
            string[] headers = Headers;
            for (int i = 1; i < headers.Length; i++)
            {
                if (headers[i] != header)
                    continue;

                return i;
            }

            return 0;
        }

#if UNITY_EDITOR
        public void Add(string key, string value, string header)
        {
            string append = string.Format("\n\"{0}\",{1}", key, AppendToCSV(value, header));
            File.AppendAllText(localisationInfo.CSVFullFilePath, append);

            UnityEditor.AssetDatabase.Refresh();
        }

        string AppendToCSV(string value, string header)
        {
            string line = "";
            string[] headers = Headers;

            for (int i = 1; i < headers.Length; ++i)
                line += (line != "" ? "," : "") + "\"" + (headers[i] == header ? value : "") + "\"";

            return line;
        }

        public void Remove(string key)
        {
            string[] lines = Lines, keys = Keys;

            int lineIndexToRemove = -1;

            for (int i = 0; i < keys.Length; ++i)
            {
                if (!keys[i].Contains(key))
                    continue;

                lineIndexToRemove = i;
                break;
            }

            if (lineIndexToRemove == -1)
                return;

            string[] newLines = lines.Where(i => i != lines[lineIndexToRemove]).ToArray();

            File.WriteAllText(localisationInfo.CSVFullFilePath, string.Join("\n", newLines));

            UnityEditor.AssetDatabase.Refresh();
        }

        public void Edit(string key, string value, string header)
        {
            string[] lines = Lines, keys = Keys;

            int fieldsIndex = IndexOfHeader(header);
            if (fieldsIndex == 0)
                return;

            int lineIndexToEdit = -1;

            for (int i = 0; i < keys.Length; ++i)
            {
                if (!keys[i].Contains(key))
                    continue;

                lineIndexToEdit = i;
                break;
            }

            if (lineIndexToEdit == -1)
                return;

            string[] fields = TrimAndSplit(lines[lineIndexToEdit]);

            fields[fieldsIndex] = value;
            lines[lineIndexToEdit] = JoinFields(fields);

            File.WriteAllText(localisationInfo.CSVFullFilePath, string.Join("\n", lines));

            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
}