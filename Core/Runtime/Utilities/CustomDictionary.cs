using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomDictionary <key, obj>
{
    [SerializeField]
    public List<key> Keys = new List<key>();
    [SerializeField]
    private List<obj> Values = new List<obj>();

    public bool ContainsKey(key key)
    {
        return Keys.Contains(key);
    }

    public bool ContainsValue(obj value)
    {
        return Values.Contains(value);
    }

    public obj GetKey(key key)
    {
        if (Keys.Contains(key))
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Equals(key))
                    return Values[i];
            }
        }
        return default;
    }

    public void Add(key key)
    {
        if (!Keys.Contains(key))
            return;
        Keys.Add(key);
        Values.Add(default);
    }

    public void Add(key key, obj value)
    {
        if (!Keys.Contains(key))
        {
            // create
            Keys.Add(key);
            Values.Add(value);
            return;
        }

        for (int i = 0; i < Keys.Count; i++)
        {
            if (!Keys[i].Equals(key))
                continue;
            Values[i] = value;
            return;
        }
    }

    public void Remove(key key)
    {
        if (!Keys.Contains(key))
            return;

        for (int i = 0; i < Keys.Count; i++)
        {
            if (!Keys[i].Equals(key))
                continue;
            Values.RemoveAt(i);
            break;
        }
        Keys.Remove(key);

    }
}