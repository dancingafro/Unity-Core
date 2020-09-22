using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace CoreScript.SaveLoad
{
    public interface IData
    {
        IData Save();
    }

    public interface ISaveFile
    {
        IData[] Datas { get; set; }
    }

    public static class SaveLoadSystem
    {
        public static string SaveFileDirectory
        {
            get
            {
#if UNITY_EDITOR
                string SaveFileDirectory = Path.Combine(Application.dataPath, "Resources/Save Files/");
#else
                string SaveFileDirectory = Path.Combine(Application.persistentDataPath, "Save Files/");
#endif
                if (!Directory.Exists(SaveFileDirectory))
                    Directory.CreateDirectory(SaveFileDirectory);

                return SaveFileDirectory;
            }
        }
        public static string AutoSaveFileDirectory
        {
            get
            {
                string autoSaveFileDirectory = Path.Combine(SaveFileDirectory, "Auto Save/");
                if (!Directory.Exists(autoSaveFileDirectory))
                    Directory.CreateDirectory(autoSaveFileDirectory);

                return autoSaveFileDirectory;
            }
        }

        static List<IData> datas = new List<IData>();

        public delegate void SerializeAction();
        public static SerializeAction OnBeforeSave;

        public static bool Save(string saveName, ISaveFile saveData, bool autoSave = false)
        {
            BinaryFormatter formatter = GetBinaryFormatter();

            string saveFilePath = GetSaveDirectories(saveName, autoSave);

            FileStream file = File.Open(saveFilePath, FileMode.OpenOrCreate);

            datas.Clear();
            OnBeforeSave?.Invoke();
            saveData.Datas = datas.ToArray();

            try
            {
                formatter.Serialize(file, saveData);
            }
            catch (Exception)
            {
                file.Close();
                return false;
            }
            file.Close();

            return true;
        }

        public static void AddToDataList(IData data)
        {
            datas.Add(data.Save());
        }

        static string GetSaveDirectories(string saveName, bool autoSave = false)
        {
            return (autoSave ? AutoSaveFileDirectory : SaveFileDirectory) + saveName + ".sav";
        }

        public static object Load<T>(string saveFilePath) where T : ISaveFile
        {
            if (!File.Exists(saveFilePath))
                return null;

            BinaryFormatter formatter = GetBinaryFormatter();

            T saveData = default;


            FileStream file = File.Open(saveFilePath, FileMode.Open);
            try
            {
                saveData = (T)formatter.Deserialize(file);
            }
            catch (Exception)
            {
                Debug.LogErrorFormat("Failed to load file at {0}", saveFilePath);
                file.Close();
                return null;
            }
            file.Close();

            return saveData;
        }

        public static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            SurrogateSelector selector = new SurrogateSelector();

            selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogates());
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new QuaternionSerializationSurrogates());

            binaryFormatter.SurrogateSelector = selector;
            return binaryFormatter;
        }

        public static string Sha256(string str)
        {
            string hash = string.Empty;
            SHA256Managed crypt = new SHA256Managed();

            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
            foreach (var item in crypto)
                hash += item.ToString("x2");

            return hash;
        }
    }
}