using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.UI
{
    public class UITabGroup : MonoBehaviour
    {
        List<UITabButton> tapButtons = null;

        [SerializeField] PanelGroup panelGroup = null;

        [SerializeField] Sprite tabIdle = null, tabHover = null, tabActive = null;

        UITabButton selectedTabButton = null;

        void Update()
        {

        }

        public void Subscribe(UITabButton tabButton)
        {
            if (tapButtons == null)
                tapButtons = new List<UITabButton>();

            tapButtons.Add(tabButton);
        }

        public void OnTabEnter(UITabButton tabButton)
        {
            if (tapButtons == null)
                return;

            if (selectedTabButton != null && selectedTabButton != tabButton)
                selectedTabButton.background.sprite = tabHover;

            ResetTabs(tabButton);
        }

        public void OnTabExit(UITabButton tabButton)
        {
            if (tapButtons == null)
                return;

            ResetTabs();
        }

        public void OnTabSelected(UITabButton tabButton)
        {
            if (tapButtons == null)
                return;

            if (selectedTabButton != null)
                selectedTabButton.Deselect();

            selectedTabButton = tabButton;
            selectedTabButton.background.sprite = tabActive;

            int index = selectedTabButton.transform.GetSiblingIndex();

            if (panelGroup != null)
                panelGroup.SetPanelIndex(selectedTabButton.transform.GetSiblingIndex());

            ResetTabs();
        }

        void ResetTabs(UITabButton tabButton = null)
        {
            foreach (var item in tapButtons)
            {
                if (item == selectedTabButton || item == tabButton)
                    continue;

                item.background.sprite = tabIdle;
            }
        }

        void SetActive(int index)
        {
            if (index < 0 || index >= tapButtons.Count)
                return;

            tapButtons[index].OnPointerClick(null);
        }

        public void ChangeTab(float dir)
        {
            if (dir > 0 && selectedTabButton.transform.GetSiblingIndex() + 1 < transform.childCount)
                SetActive(selectedTabButton.transform.GetSiblingIndex() + 1);
            else if (selectedTabButton.transform.GetSiblingIndex() - 1 > -1)
                SetActive(selectedTabButton.transform.GetSiblingIndex() - 1);
        }
    }
}
