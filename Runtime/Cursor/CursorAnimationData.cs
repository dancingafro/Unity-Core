using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Cursors
{
    [System.Serializable]
    public class CursorAnimationData
    {
        public string Name
        {
            get
            {
                return (cursorType == null) ? "" : cursorType.name;
            }
        }
        [SerializeField] CursorType cursorType = null;
        [SerializeField] Texture2D[] listOfCursorTexture;
        [SerializeField] float frameRate = .1f;
        [SerializeField] Vector2 hotSpot = Vector2.zero;
        int currentFrame = 0;

        public CursorAnimationData(CursorType cursorType)
        {
            this.cursorType = cursorType;
            listOfCursorTexture = new Texture2D[1];
        }

        [HideInInspector]
        public Texture2D this[int i] { get { return listOfCursorTexture[i]; } }
        public float FrameRate { get { return frameRate; } }
        public Vector2 HotSpot { get { return hotSpot; } }
        public CursorType CursorType { get { return cursorType; } }

        public Texture2D CurrentFrame
        {
            get
            {
                if (listOfCursorTexture == null || listOfCursorTexture.Length == 0)
                    return null;
                return listOfCursorTexture[currentFrame];
            }
        }

        public void ResetData()
        {
            currentFrame = 0;
        }

        public Texture2D GetNextFrame()
        {
            if (listOfCursorTexture == null || listOfCursorTexture.Length == 0)
                return null;
            currentFrame = (currentFrame + 1) % listOfCursorTexture.Length;
            return listOfCursorTexture[currentFrame];
        }

        void OnValidate()
        {
            if (frameRate < .0001f)
                frameRate = .0001f;
        }
    }
}