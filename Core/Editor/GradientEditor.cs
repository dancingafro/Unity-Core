using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Utility
{
    public class GradientEditor : EditorWindow
    {
        public CustomColourGradient Gradient { private get; set; }
        const int borderSize = 10;
        const float keyWidth = 10;
        const float keyHeight = 20;

        Rect[] keysRect;
        bool mouseIsDownOverKey = false, needsRepaint = false;
        int selectedIndex = 0;
        Event guiEvent;
        Rect gradientPreviewRect;
        private void OnGUI()
        {
            Draw();
            HandleInput();
            if (needsRepaint)
            {
                needsRepaint = false;
                Repaint();
            }
        }

        void Draw()
        {
            gradientPreviewRect = new Rect(borderSize, borderSize, position.width - borderSize * 2, 25);
            GUI.DrawTexture(gradientPreviewRect, Gradient.GetTexture((int)gradientPreviewRect.width));
            keysRect = new Rect[Gradient.NumKey];
            for (int i = 0; i < Gradient.NumKey; ++i)
            {
                ColorKey key = Gradient.GetKey(i);
                keysRect[i] = new Rect(gradientPreviewRect.x + gradientPreviewRect.width * key.Time - keyWidth * .5f, gradientPreviewRect.yMax + borderSize, keyWidth, keyHeight);

                if (i == selectedIndex)
                    EditorGUI.DrawRect(new Rect(keysRect[i].x - 2, keysRect[i].y - 2, keysRect[i].width + 4, keysRect[i].height + 4), Color.black);

                EditorGUI.DrawRect(keysRect[i], key.Colour);
            }

            Rect settingRect = new Rect(borderSize, keysRect[0].yMax + borderSize, position.width - borderSize * 2, position.height);
            GUILayout.BeginArea(settingRect);

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField("Name :",Gradient.GetKey(selectedIndex).Name);
            if (EditorGUI.EndChangeCheck())
                Gradient.UpdateKeyName(selectedIndex, newName);

            EditorGUI.BeginChangeCheck();
            Color newColour = EditorGUILayout.ColorField("Colour :", Gradient.GetKey(selectedIndex).Colour);
            if (EditorGUI.EndChangeCheck())
                Gradient.UpdateKeyColor(selectedIndex, newColour);

            EditorGUI.BeginChangeCheck();
            float newTime = EditorGUILayout.FloatField("Time :", Gradient.GetKey(selectedIndex).Time);
            if (EditorGUI.EndChangeCheck())
                selectedIndex = Gradient.UpdateKeyTime(selectedIndex, newTime);

            Gradient.blendMode = (CustomColourGradient.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", Gradient.blendMode);

            Gradient.randomizeColourOnAdd = EditorGUILayout.Toggle("Randomize Colour on Add", Gradient.randomizeColourOnAdd);

            GUILayout.EndArea();
        }

        void HandleInput()
        {
            guiEvent = Event.current;
            if (IsMouseEvent(EventType.MouseDown, 0))
            {
                for (int i = 0; i < keysRect.Length; ++i)
                {
                    if (!keysRect[i].Contains(guiEvent.mousePosition))
                        continue;

                    mouseIsDownOverKey = true;
                    selectedIndex = i;
                    needsRepaint = true;
                    break;
                }

                if (!mouseIsDownOverKey)
                {
                    float keyTime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                    Color newColour = Gradient.randomizeColourOnAdd ? new Color(Random.value, Random.value, Random.value) : Gradient.Evaluate(keyTime);

                    selectedIndex = Gradient.AddKey(newColour, keyTime);
                    mouseIsDownOverKey = true;
                    needsRepaint = true;
                }
            }

            if (IsMouseEvent(EventType.MouseUp, 0))
                mouseIsDownOverKey = false;

            if (mouseIsDownOverKey && IsMouseEvent(EventType.MouseDrag, 0))
            {
                float keyTime = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                selectedIndex = Gradient.UpdateKeyTime(selectedIndex, keyTime);
                needsRepaint = true;
            }

            if (IsKeyboardEvent(KeyCode.Backspace | KeyCode.Delete, EventType.KeyDown))
            {
                Gradient.RemoveKey(selectedIndex);
                selectedIndex = Mathf.Clamp(selectedIndex, 0, Gradient.NumKey - 1);
                needsRepaint = true;
            }

        }

        bool IsMouseEvent(EventType type, int buttonIndex)
        {
            return IsEventType(type) && guiEvent.button == buttonIndex;
        }

        bool IsKeyboardEvent(KeyCode keyCode, EventType type)
        {
            return guiEvent.keyCode == keyCode && IsEventType(type);
        }

        bool IsEventType(EventType type)
        {
            return guiEvent.type == type;
        }

        private void OnEnable()
        {
            selectedIndex = 0;
            titleContent.text = "Gradient Editor";
            position.Set(position.x, position.y, 400, 150);
            minSize = new Vector2(200, 200);
            maxSize = new Vector2(1920, 200);
        }

        private void OnDisable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }


    }
}