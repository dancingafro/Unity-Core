using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    [System.Serializable]
    public class CustomColourGradient : IGradient<ColorKey>
    {
        public enum BlendMode
        {
            Linear,
            Discrete
        }
        public BlendMode blendMode;
        public bool randomizeColourOnAdd = false;

        [SerializeField]
        List<ColorKey> keys = new List<ColorKey>();

        public CustomColourGradient()
        {
            AddKey(Color.white, 0);
            AddKey(Color.black, 1);
        }

        public int NumKey { get { return keys.Count; } }

        public int AddKey(Color colour, float time, string name = "")
        {
            return AddKey(new ColorKey(colour, time, name));
        }

        public int AddKey(ColorKey colorKey)
        {
            for (int i = 0; i < NumKey; i++)
            {
                if (colorKey.Time > keys[i].Time)
                    continue;

                keys.Insert(i, colorKey);
                return i;
            }

            keys.Add(colorKey);
            return keys.Count - 1;
        }

        public void RemoveKey(int index)
        {
            if (keys.Count < 2)
                return;
            keys.RemoveAt(index);
        }

        public int UpdateKeyTime(int index, float time)
        {
            Color colour = keys[index].Colour;
            string name = keys[index].Name;
            RemoveKey(index);
            return AddKey(colour, time, name);
        }

        public void UpdateKeyName(int index, string name)
        {
            keys[index] = new ColorKey(keys[index].Colour, keys[index].Time, name);
        }

        public void UpdateKeyColor(int index, Color colour)
        {
            keys[index] = new ColorKey(colour, keys[index].Time, keys[index].Name);
        }

        public int UpdateKey(int index, Color colour, float time, string name = "")
        {
            UpdateKeyColor(index, colour);
            UpdateKeyName(index, name);
            return UpdateKeyTime(index, time);
        }

        public Color Evaluate(float time)
        {
            if (keys.Count == 0)
                return Color.white;

            int keyLeft = 0, keyRight = NumKey - 1;

            for (int i = 0; i < NumKey; ++i)
            {
                if (keys[i].Time > time && keys[i].Time < time)
                    continue;

                if (keys[i].Time <= time)
                    keyLeft = i;
                else
                {
                    keyRight = i;
                    break;
                }
            }

            switch (blendMode)
            {
                case BlendMode.Discrete:
                    return keys[keyRight].Colour;
            }
            return Color.Lerp(keys[keyLeft].Colour, keys[keyRight].Colour, Mathf.InverseLerp(keys[keyLeft].Time, keys[keyRight].Time, time));
        }

        public ColorKey GetKey(int index)
        {
            return keys[index];
        }

        public Texture2D GetTexture(int width)
        {
            Color[] colors = new Color[width];
            for (int x = 0; x < width; x++)
                colors[x] = Evaluate((float)x / (width - 1));

            return UtilityCode.TextureFromColors(colors, width, 1);
        }
    }

    public interface IGradient<T>
    {
        int AddKey(Color colour, float time, string name = "");
        int AddKey(T colorKey);
        void RemoveKey(int index);
        int UpdateKeyTime(int index, float time);
        void UpdateKeyName(int index, string name);
        void UpdateKeyColor(int index, Color colour);
        int UpdateKey(int index, Color colour, float time, string name = "");
        Color Evaluate(float time);
        T GetKey(int index);
        Texture2D GetTexture(int width);
    }

    [System.Serializable]
    public struct ColorKey
    {
        [SerializeField] string name;
        [SerializeField] Color colour;
        [SerializeField] float time;

        public ColorKey(Color colour, float time, string name = "")
        {
            this.name = name;
            this.colour = colour;
            this.time = time;
        }

        public Color Colour { get { return colour; } }
        public float Time { get { return time; } }
        public string Name { get { return name; } }
    }

}