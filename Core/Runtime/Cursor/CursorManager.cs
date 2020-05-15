using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Singleton;

namespace CoreScript.Cursors
{
    public class CursorManager : Singleton<CursorManager>
    {
        [SerializeField] CursorManagerData mouseManagerData;
        float timer = 0f;
        CursorAnimationData currentCursorAnimation;

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
            mouseManagerData.SetActiveCursorAnimation(cursorType);
            timer = mouseManagerData.CurrentCursorAnimation.FrameRate;
            Cursor.SetCursor(mouseManagerData.CurrentCursorAnimation[0], mouseManagerData.CurrentCursorAnimation.HotSpot, CursorMode.Auto);
        }

        public void SetDefaultCursorAnimation()
        {
            mouseManagerData.SetActiveCursorAnimation();
            timer = mouseManagerData.CurrentCursorAnimation.FrameRate;
            Cursor.SetCursor(mouseManagerData.CurrentCursorAnimation[0], mouseManagerData.CurrentCursorAnimation.HotSpot, CursorMode.Auto);
        }
    }
}
