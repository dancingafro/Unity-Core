using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomScript.UI
{
    public class PanelGroup : MonoBehaviour
    {
        [SerializeField] List<GameObject> panels = new List<GameObject>();
        [SerializeField] UITabGroup tabGroup;

        int panelIndex;

        void Awake()
        {
            ShowCurrentIndex();
        }

        void ShowCurrentIndex()
        {
            for (int i = 0; i < panels.Count; i++)
                panels[i].SetActive(i == panelIndex);
        }

        public void SetPanelIndex(int index)
        {
            if (index < 0 || index > panels.Count - 1)
                return;

            panelIndex = index;
            ShowCurrentIndex();
        }
    }
}