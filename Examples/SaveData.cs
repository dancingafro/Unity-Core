using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.SaveLoad;

[System.Serializable]
public class SaveData : ISaveFile
{
    public Vector3 position;
    public Quaternion rotation;

    public IData[] Datas { get; set; } = null;
}
