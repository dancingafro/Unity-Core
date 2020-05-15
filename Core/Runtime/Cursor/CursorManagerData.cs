using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Cursors
{
    //[CreateAssetMenu(fileName = "MouseManagerData", menuName = "Mouse/Mouse Manager Data")]
    public class CursorManagerData : ScriptableObject
    {
        public Dictionary<CursorType, CursorAnimationData> cursorAnimations;
        [SerializeField] CursorType defaultCursorType;
        CursorType currentCursorType;

        public CursorAnimationData CurrentCursorAnimation { get { return cursorAnimations[currentCursorType]; } }

        public void SetActiveCursorAnimation()
        {
            SetActiveCursorAnimation(defaultCursorType);
        }

        public void SetActiveCursorAnimation(CursorType cursorType)
        {
            if (!cursorAnimations.ContainsKey(cursorType))
                return;
            currentCursorType = cursorType;
            CurrentCursorAnimation.ResetData();
        }

#if UNITY_EDITOR
        public void AddNewCursorAnimation(CursorType cursorType, CursorAnimationData cursorAnimationData)
        {
            cursorAnimations.Add(cursorType, cursorAnimationData);
        }

        public static CursorManagerData Load()
        {
            CursorManagerData cursorManagerData = Resources.Load<CursorManagerData>("Resources/CoreScript/Cursor/CursorManagerData.asset");
            if (cursorManagerData != null)
                return cursorManagerData;

            cursorManagerData = ScriptableObject.CreateInstance<CursorManagerData>();
            UnityEditor.AssetDatabase.CreateAsset(cursorManagerData, "Assets/com.desmond.corescript/Core/Resources/CoreScript/Cursor/CursorManagerData.asset");
            Debug.LogWarning("Could not find CursorManagerData asset. Will use default settings instead.");
            return cursorManagerData;
        }
#endif
    }
}