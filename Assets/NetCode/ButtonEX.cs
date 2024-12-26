using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// UNITY增强
/// </summary>
namespace LT_Code.UnityExtra
{
    /// <summary>
    /// 绘梦璃的增强按钮
    /// </summary>
    [AddComponentMenu("UI/ButtonEX", 31)]
    public class ButtonEX : Selectable, IPointerClickHandler, ISubmitHandler
    {
        /// <summary>
        /// 长按检测时间
        /// </summary>
        public float LongPressTime = 0.5f;

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("MouseDown")]
        /// #endif
        public ButtonMouseDownEvent mouseDown = new ButtonMouseDownEvent();

        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR

        [SerializeField]
        [FormerlySerializedAs("MouseUp")]
        /// #endif
        public ButtonMouseUpEvent mouseUp = new ButtonMouseUpEvent();

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("MoseMove")]
        /// #endif
        public ButtonMouseMoveEvent mouseMove = new ButtonMouseMoveEvent();

        /// <summary>
        /// 鼠标移入事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("MouseEnter")]
        /// #endif
        public ButtonMouseEnterEvent mouseEnter = new ButtonMouseEnterEvent();

        /// <summary>
        /// 鼠标移出事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("MouseOut")]
        /// #endif
        public ButtonMouseOutEvent mouseOut = new ButtonMouseOutEvent();

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("OnClick")]
        /// #endif
        public ButtonClieckedEvent onClick = new ButtonClieckedEvent();

        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("DoubleClick")]
        /// #endif
        public ButtonDoubleClick doubleClick = new ButtonDoubleClick();

        /// <summary>
        /// 鼠标长按事件
        /// </summary>
        /// #if UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID || UNITY_EDITOR
        [SerializeField]
        [FormerlySerializedAs("LongPress")]
        /// #endif
        public ButtonLongPressEvent longPress = new ButtonLongPressEvent();

        /// <summary>
        /// 是否还按着按钮
        /// </summary>
        protected bool isMouseDown = false;

        /// <summary>
        /// 是否已经触发了长按功能
        /// </summary>
        protected bool isLongPress = false;

        /// <summary>
        /// 按住的时间
        /// </summary>
        protected float pressTime = 0f;

        /// <summary>
        /// 鼠标是否在按钮内
        /// </summary>
        protected bool isMouseIn = false;

        /// <summary>
        /// 鼠标位置
        /// </summary>
        protected Vector2 mousePostion = new Vector2();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (!isMouseDown)
            {
                //双击
                if (eventData.clickCount == 2)
                {
                    if (doubleClick != null)
                    {
                        doubleClick.Invoke();
                    }
                }
                else if (eventData.clickCount == 1)
                {
                    onClick.Invoke();
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            mouseDown.Invoke();
            isMouseDown = true;
            pressTime = Time.time;
        }

        public void CheckLongPress()
        {
            if (isMouseDown)
            {
                if (Time.time > LongPressTime + pressTime && !isLongPress)
                {
                    isLongPress = true;
                    longPress.Invoke();
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (isMouseDown)
            {
                isMouseDown = false;
                isLongPress = false;
                mouseUp.Invoke();
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            mouseOut.Invoke();
            isMouseIn = false;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            mouseEnter.Invoke();
            isMouseIn = true;
            mousePostion = eventData.position;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.mouseUp", this);
            mouseUp.Invoke();
            onClick.Invoke();
        }

        void Update()
        {
            if (!IsActive() || !IsInteractable())
                return;

            //判断长按
            CheckLongPress();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            var nowPosition = eventData.position;
            if (mousePostion != nowPosition)
            {
                mousePostion = nowPosition;
                mouseMove.Invoke();
            }
        }

        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        [Serializable]
        public class ButtonMouseDownEvent : UnityEvent { }

        /// <summary>
        /// 鼠标弹起事件
        /// </summary>
        [Serializable]
        public class ButtonMouseUpEvent : UnityEvent { }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        [Serializable]
        public class ButtonMouseMoveEvent : UnityEvent { }

        /// <summary>
        /// 鼠标移入事件
        /// </summary>
        [Serializable]
        public class ButtonMouseEnterEvent : UnityEvent { }

        /// <summary>
        /// 鼠标移出事件
        /// </summary>
        [Serializable]
        public class ButtonMouseOutEvent : UnityEvent { }

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        [Serializable]
        public class ButtonClieckedEvent : UnityEvent { }

        /// <summary>
        /// 鼠标双击事件
        /// </summary>
        [Serializable]
        public class ButtonDoubleClick : UnityEvent { }

        /// <summary>
        /// 鼠标长按事件
        /// </summary>
        [Serializable]
        public class ButtonLongPressEvent : UnityEvent { }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI/ButtonEX")]
        public static void AddNewButtonEX()
        {

            var newButtonEx = new GameObject("ButtonEX",
                typeof(RectTransform),
                typeof(Image),
                typeof(ButtonEX)
            );
            if (GameObject.Find("Canvas") == null)
            {
                var canvas = new GameObject("Canvas",
                    typeof(Canvas),
                    typeof(RectTransform),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );  //创建一个GameObject  加入Canvas的组件
            }
            else
            {
                newButtonEx.transform.SetParent(Selection.activeTransform, true);
            }
            //if (GameObject.Find("EventSystem"))
            //{
            //    var eventSystem = new GameObject("EventSystem",
            //        typeof(EventSystem),
            //        typeof(StandaloneInputModule)
            //    );
            //}
            var buttonRect = newButtonEx.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(160, 30);
            buttonRect.localPosition = Vector3.zero;
            var text = new GameObject("Text",
                typeof(RectTransform),
                typeof(Text)
            );
            var textRect = text.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(1600, 300);
            textRect.localScale = new Vector3(0.1f, 0.1f, 1);
            var theText = text.GetComponent<Text>();
            theText.text = "ButtonEX";
            theText.color = new Color(0, 0, 0);
            theText.alignment = TextAnchor.MiddleCenter;
            theText.fontSize = 200;
            text.transform.SetParent(newButtonEx.transform);
            text.transform.localPosition = Vector3.zero;
        }
#endif
    }
}
