using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Localisation
{
    [CreateAssetMenu(fileName = "Language", menuName = "Localisation/Language", order = 0)]
    public class Languages : ScriptableObject
    {
        [SerializeField] string key = "";
        [SerializeField] bool defaultLanguage = false;
        public string Key { get { return key; } }
        public bool DefaultLanguage { get { return defaultLanguage; } }
    }
}