using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Cursors
{
    [CreateAssetMenu(fileName = "Cursor Animation Data", menuName = "Cursor/Cursor Animation Data")]
    public class CursorAnimationData : ScriptableObject
    {
        [SerializeField] CursorType cursorType;
        [SerializeField] List<Texture2D> listOfCursorTexture = new List<Texture2D>();
        [SerializeField] float frameRate = .1f;
        [SerializeField] Vector2 hotSpot;
        int currentFrame = 0;

        public Texture2D this[int i] { get { return listOfCursorTexture[i]; } }
        public float FrameRate { get { return frameRate; } }
        public Vector2 HotSpot { get { return hotSpot; } }

        public void ResetData()
        {
            currentFrame = 0;
        }

        public Texture2D GetNextFrame()
        {
            currentFrame = (currentFrame + 1) % listOfCursorTexture.Count;
            return listOfCursorTexture[currentFrame];
        }

        void OnValidate()
        {
            if (frameRate < .0001f)
                frameRate = .0001f;
        }
    }
}