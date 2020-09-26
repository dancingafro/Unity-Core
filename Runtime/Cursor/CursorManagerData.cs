using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoreScript.Cursors
{
    //[CreateAssetMenu(fileName = "MouseManagerData", menuName = "Mouse/Mouse Manager Data")]
    public class CursorManagerData : ScriptableObject
    {
        public CursorAnimationData[] cursorAnimations = new CursorAnimationData[1];
        public CursorType defaultCursorType;
        CursorType currentCursorType;
        public CursorType CurrentCursorType
        {
            get
            {
                if (!currentCursorType)
                    currentCursorType = defaultCursorType;

                return currentCursorType;
            }
            private set { currentCursorType = value; }
        }

        public CursorAnimationData CurrentCursorAnimation
        {
            get
            {
                CursorAnimationData temp = null;
                foreach (var item in cursorAnimations)
                {
                    if (item.CursorType == CurrentCursorType)
                        return item;
                    else if (item.CursorType == defaultCursorType)
                        temp = item;
                }
                return temp;
            }
        }

        public void SetDefaultCursorAnimation()
        {
            SetActiveCursorAnimation(defaultCursorType);
        }

        public void SetActiveCursorAnimation(CursorType cursorType)
        {
            if (!ContainsType(cursorType))
                return;
            CurrentCursorType = cursorType;
            CurrentCursorAnimation.ResetData();
        }

        public void SetActiveCursorAnimation(int index)
        {
            SetActiveCursorAnimation(cursorAnimations[index].CursorType);
        }

        bool ContainsType(CursorType cursorType)
        {
            foreach (var item in cursorAnimations)
            {
                if (item.CursorType == cursorType)
                    return true;
            }
            return false;
        }

        public static CursorManagerData Load()
        {
            string path = "CoreScript/Cursor";
            CursorManagerData cursorManagerData = Resources.Load<CursorManagerData>(path + "/CursorManagerData");
            if (cursorManagerData != null)
                return cursorManagerData;
#if UNITY_EDITOR
            string additionalPath = Application.dataPath + "/Resources/" + path;
            cursorManagerData = CreateInstance<CursorManagerData>();

            if (!Directory.Exists(additionalPath))
                Directory.CreateDirectory(additionalPath);

            UnityEditor.AssetDatabase.CreateAsset(cursorManagerData, "Assets/Resources/" + path + "/CursorManagerData.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.LogWarning("Could not find CursorManagerData asset. Will use default settings instead.");
            return cursorManagerData;
#else
            return null;
#endif
        }

#if UNITY_EDITOR
        public void AddNewCursorAnimation(CursorAnimationData cursorAnimationData)
        {
            CursorAnimationData[] temp = cursorAnimations;
            cursorAnimations = new CursorAnimationData[temp.Length + 1];

            for (int i = 0; i < temp.Length; i++)
                cursorAnimations[i] = temp[i];

            cursorAnimations[cursorAnimations.Length - 1] = cursorAnimationData;
        }

        public bool Contain(CursorType cursorType)
        {
            foreach (var item in cursorAnimations)
            {
                if (item.CursorType == cursorType)
                    return true;
            }
            return false;
        }
        public void RemoveAt(int index)
        {
            CursorAnimationData[] temp = cursorAnimations;
            cursorAnimations = new CursorAnimationData[temp.Length - 1];

            for (int i = 0; i < temp.Length; i++)
            {
                if (i == index)
                    continue;
                cursorAnimations[i] = temp[i];
            }
        }
#endif
    }
}