using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoreScript.Cursors
{
    public class CursorObjectUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] CursorType cursorType = null;

        public void OnPointerEnter(PointerEventData eventData)
        {
            CursorManager.Instance.SetActiveCursorAnimation(cursorType, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CursorManager.Instance.SetDefaultCursorAnimation(true);
        }
    }
}