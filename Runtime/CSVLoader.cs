using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CoreScript.Utility
{
    public class CSVLoader
    {
        string[] csvFile;
        char surround = '"';
        string[] fieldSeperator = { "\",\"" };

        public CSVLoader(string[] lines)
        {
            csvFile = lines;
        }

        public Dictionary<string, Dictionary<string, string>> GetDictionaryValues()
        {
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();

            string[] headers = csvFile[0].Split(fieldSeperator, StringSplitOptions.None);

            for (int i = 1; i < headers.Length; i++)
                dictionary.Add(headers[i], new Dictionary<string, string>());

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for (int i = 0; i < csvFile.Length; i++)
            {
                string[] fields = CSVParser.Split(csvFile[i]);
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
    }
}