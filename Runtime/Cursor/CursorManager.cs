using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Singleton;

namespace CoreScript.Cursors
{
    public class CursorManager : Singleton<CursorManager>
    {
        CursorManagerData cursorManagerData = null;
        public CursorManagerData CursorManagerData
        {
            get
            {
                if (cursorManagerData == null)
                    cursorManagerData = CursorManagerData.Load();

                return cursorManagerData;
            }
        }
        float timer = 0f;
        CursorAnimationData currentCursorAnimation = null;

        bool onUI = false;

        public CursorAnimationData CurrentCursorAnimation
        {
            get
            {
                if (currentCursorAnimation == null)
                    currentCursorAnimation = CursorManagerData.CurrentCursorAnimation;

                return currentCursorAnimation;
            }
            set { currentCursorAnimation = value; }
        }

        // Update is called once per frame
        void Update()
        {
            timer -= Time.unscaledDeltaTime;

            if (timer > 0f)
                return;

            timer += CurrentCursorAnimation.FrameRate;
            Cursor.SetCursor(CurrentCursorAnimation.GetNextFrame(), CurrentCursorAnimation.HotSpot, CursorMode.Auto);

        }

        public void SetActiveCursorAnimation(CursorType cursorType, bool isUI = false)
        {
            if (onUI && !isUI)
                return;

            CursorManagerData.SetActiveCursorAnimation(cursorType);
            CurrentCursorAnimation = CursorManagerData.CurrentCursorAnimation;
            timer = CursorManagerData.CurrentCursorAnimation.FrameRate;
            Cursor.SetCursor(CursorManagerData.CurrentCursorAnimation[0], CursorManagerData.CurrentCursorAnimation.HotSpot, CursorMode.Auto);
        }

        public void SetDefaultCursorAnimation(bool isUI = false)
        {
            SetActiveCursorAnimation(CursorManagerData.defaultCursorType, isUI);
        }
    }
}
