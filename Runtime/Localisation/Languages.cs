using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Localisation
{
    [CreateAssetMenu(fileName = "Language", menuName = "Localisation/Language", order = 0)]
    public class Languages : ScriptableObject
    {
        [SerializeField] string header = "";
        [SerializeField] bool defaultLanguage = false;
        public string Header
        {
            get { return header; }
#if UNITY_EDITOR
            set { header = value; }
#endif
        }
        public bool DefaultLanguage
        {
            get { return defaultLanguage; }
#if UNITY_EDITOR
            set { defaultLanguage = value; }
#endif
        }
    }
}