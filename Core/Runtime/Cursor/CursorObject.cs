using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Cursors
{
    public class CursorObject : MonoBehaviour
    {
        [SerializeField] CursorType cursorType;

        void OnMouseEnter()
        {
            CursorManager.Instance.SetActiveCursorAnimation(cursorType);
        }

        void OnMouseExit()
        {
            CursorManager.Instance.SetDefaultCursorAnimation();
        }
    }
}