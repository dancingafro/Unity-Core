using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomScript.UI.Animate
{
    public enum UIAnimateType
    {
        Move,
        Scale,
        ScaleX,
        ScaleY,
        Fade
    }

    public class UITweening : MonoBehaviour
    {

        [SerializeField] GameObject objectToAnimate = null;
        [SerializeField] UIAnimateType animateType = UIAnimateType.Move;
        [SerializeField] LeanTweenType easeType;
        [SerializeField] float duration = 1, delay = 1;
        [SerializeField] bool loop = false, pingPong = false, startPositionOffset = false;
        [SerializeField] Vector3 from = Vector3.zero, to = Vector3.zero;

        LTDescr tweenObject = null;

        [SerializeField] bool showOnEnable = false, workOnEnable = false;

        [SerializeField] UnityEngine.Events.UnityEvent onComplete = null;

        void OnEnable()
        {
            if (showOnEnable || workOnEnable)
                Show();
        }

        void Show()
        {
            HandleTween();
        }

        void HandleTween()
        {
            if (objectToAnimate == null)
                objectToAnimate = gameObject;

            switch (animateType)
            {
                case UIAnimateType.Move:
                    MoveAbsolute();
                    break;
                case UIAnimateType.Scale:
                case UIAnimateType.ScaleX:
                case UIAnimateType.ScaleY:
                    Scale();
                    break;
                case UIAnimateType.Fade:
                    Fade();
                    break;
            }

            tweenObject.setDelay(delay);
            tweenObject.setEase(easeType);

            if (loop)
                tweenObject.loopCount = int.MaxValue;
            if (pingPong)
                tweenObject.setLoopPingPong();
        }

        void Fade()
        {
            if (GetComponent<CanvasGroup>() == null)
                gameObject.AddComponent<CanvasGroup>();

            CanvasGroup canvasGroup = objectToAnimate.GetComponent<CanvasGroup>();

            if (startPositionOffset)
                canvasGroup.alpha = from.x;

            tweenObject = LeanTween.alphaCanvas(canvasGroup, to.x, duration);
        }

        void MoveAbsolute()
        {
            RectTransform rectTransform = objectToAnimate.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = from;

            tweenObject = LeanTween.move(rectTransform, to, duration);
        }

        void Scale()
        {
            if (startPositionOffset)
                objectToAnimate.GetComponent<RectTransform>().localScale = from;

            tweenObject = LeanTween.scale(objectToAnimate, to, duration);
        }

        void Swap()
        {
            Vector3 temp = from;
            from = to;
            to = temp;
        }

        public void Disable()
        {
            Swap();
            HandleTween();

            tweenObject.setOnComplete(() =>
            {
                Swap();
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
        }

    }
}