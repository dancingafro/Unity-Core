using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreScript.UI
{
    [RequireComponent(typeof(Image))]
    public class UITabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] UITabGroup tapGroup = null;

        public Image background = null;

        [SerializeField] List<Graphic> graphics = null;
        [SerializeField] List<Color> activeColors = null;
        [SerializeField] List<Color> deactiveColors = null;

        public UnityEvent onTabSelected, onTabDeselected, onTabHover;

        void Awake()
        {
            background = GetComponent<Image>();
        }

        void Start()
        {
            if (tapGroup == null)
                tapGroup = transform.GetComponentInParent<UITabGroup>();

            tapGroup.Subscribe(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tapGroup.OnTabSelected(this);
            Select();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tapGroup.OnTabEnter(this);
            Hover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tapGroup.OnTabExit(this);
        }

        public void Hover()
        {
            onTabHover?.Invoke();
        }

        public void Select()
        {
            onTabSelected?.Invoke();
            for (int i = 0; i < graphics.Count; i++)
                graphics[i].color = activeColors[i];
        }

        public void Deselect()
        {
            onTabDeselected?.Invoke();

            for (int i = 0; i < graphics.Count; i++)
                graphics[i].color = deactiveColors[i];
        }
    }
}
