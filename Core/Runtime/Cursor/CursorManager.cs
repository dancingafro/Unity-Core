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

        // Update is called once per frame
        void Update()
        {
            timer -= Time.unscaledDeltaTime;

            if (timer > 0f)
                return;

            timer += currentCursorAnimation.FrameRate;
            Cursor.SetCursor(currentCursorAnimation.GetNextFrame(), currentCursorAnimation.HotSpot, CursorMode.Auto);

        }

        public void SetActiveCursorAnimation(CursorType cursorType)
        {
            CursorManagerData.SetActiveCursorAnimation(cursorType);
            currentCursorAnimation = CursorManagerData.CurrentCursorAnimation;
            timer = CursorManagerData.CurrentCursorAnimation.FrameRate;
            Cursor.SetCursor(CursorManagerData.CurrentCursorAnimation[0], CursorManagerData.CurrentCursorAnimation.HotSpot, CursorMode.Auto);
        }

        public void SetDefaultCursorAnimation()
        {
            SetActiveCursorAnimation(CursorManagerData.defaultCursorType);
        }
    }
}
