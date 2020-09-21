using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Cursors
{
    //[CreateAssetMenu(fileName = "MouseManagerData", menuName = "Mouse/Mouse Manager Data")]
    public class CursorManagerData : ScriptableObject
    {
        public CursorAnimationData[] cursorAnimations = new CursorAnimationData[1];
        public CursorType defaultCursorType;
        public CursorType CurrentCursorType { get; private set; }

        public CursorAnimationData CurrentCursorAnimation
        {
            get
            {
                foreach (var item in cursorAnimations)
                {
                    if (item.CursorType == CurrentCursorType)
                        return item;
                }
                return null;
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
            CursorManagerData cursorManagerData = Resources.Load<CursorManagerData>("CoreScript/Cursor/CursorManagerData");
            if (cursorManagerData != null)
                return cursorManagerData;
#if UNITY_EDITOR
            cursorManagerData = ScriptableObject.CreateInstance<CursorManagerData>();
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/CoreScript/Cursor"))
            {
                string[] paths = "Assets/Resources/CoreScript/Cursor".Split('/');
                for (int i = 1; i < paths.Length; i++)
                {
                    if (UnityEditor.AssetDatabase.IsValidFolder(paths[i]))
                        continue;

                    UnityEditor.AssetDatabase.CreateFolder(paths[i - 1], paths[i]);
                }
            }
            UnityEditor.AssetDatabase.CreateAsset(cursorManagerData, "Assets/Resources/CoreScript/Cursor/CursorManagerData.asset");
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