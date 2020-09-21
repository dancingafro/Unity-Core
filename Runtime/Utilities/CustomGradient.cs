using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    [System.Serializable]
    public class CustomColourGradient : CustomGradient<ColorKey>
    {
        public bool randomizeColourOnAdd = false;

        public CustomColourGradient()
        {
            AddKey(Color.white, 0);
            AddKey(Color.black, 1);
        }

        public int AddKey(Color colour, float time, string name = "")
        {
            return AddKey(new ColorKey(colour, time, name));
        }

        public override int AddKey(ColorKey colorKey)
        {
            for (int i = 0; i < NumKey; i++)
            {
                if (colorKey.Time > Keys[i].Time)
                    continue;

                Keys.Insert(i, colorKey);
                return i;
            }

            Keys.Add(colorKey);
            return Keys.Count - 1;
        }

        public override void RemoveKey(int index)
        {
            if (Keys.Count < 2)
                return;
            Keys.RemoveAt(index);
        }

        public int UpdateKeyTime(int index, float time)
        {
            Color colour = Keys[index].Colour;
            string name = Keys[index].Name;
            RemoveKey(index);
            return AddKey(colour, time, name);
        }

        public void UpdateKeyName(int index, string name)
        {
            Keys[index] = new ColorKey(Keys[index].Colour, Keys[index].Time, name);
        }

        public void UpdateKeyColor(int index, Color colour)
        {
            Keys[index] = new ColorKey(colour, Keys[index].Time, Keys[index].Name);
        }

        public int UpdateKey(int index, Color colour, float time, string name = "")
        {
            UpdateKeyColor(index, colour);
            UpdateKeyName(index, name);
            return UpdateKeyTime(index, time);
        }

        public override Color Evaluate(float time)
        {
            if (Keys.Count == 0)
                return Color.white;

            int keyLeft = 0, keyRight = NumKey - 1;

            for (int i = 0; i < NumKey; ++i)
            {
                if (Keys[i].Time > time && Keys[i].Time < time)
                    continue;

                if (Keys[i].Time <= time)
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
                    return Keys[keyRight].Colour;
            }
            return Color.Lerp(Keys[keyLeft].Colour, Keys[keyRight].Colour, Mathf.InverseLerp(Keys[keyLeft].Time, Keys[keyRight].Time, time));
        }

        public override ColorKey GetKey(int index)
        {
            return Keys[index];
        }

        public override Texture2D GetTexture(int width)
        {
            Color[] colors = new Color[width];
            for (int x = 0; x < width; x++)
                colors[x] = Evaluate((float)x / (width - 1));

            return UtilityClass.TextureFromColors(colors, width, 1);
        }

        public override int UpdateKey(int index, ColorKey Key)
        {
            UpdateKeyColor(index, Key.Colour);
            UpdateKeyName(index, Key.Name);
            return UpdateKeyTime(index, Key.Time);
        }
    }

    [System.Serializable]
    public class CustomTextureGradient : CustomGradient<TextureKey>
    {
        public CustomTextureGradient()
        {

            AddKey(Texture2D.blackTexture, 0);
            AddKey(Texture2D.whiteTexture, 1);
        }

        public int AddKey(Texture2D texture, float time, string name = "")
        {
            return AddKey(new TextureKey(texture, time, name));
        }

        public override int AddKey(TextureKey colorKey)
        {
            for (int i = 0; i < NumKey; i++)
            {
                if (colorKey.Time > Keys[i].Time)
                    continue;

                Keys.Insert(i, colorKey);
                return i;
            }

            Keys.Add(colorKey);
            return Keys.Count - 1;
        }

        public override void RemoveKey(int index)
        {
            if (Keys.Count < 2)
                return;
            Keys.RemoveAt(index);
        }

        public int UpdateKeyTime(int index, float time)
        {
            Texture2D texture = Keys[index].Texture2D;
            string name = Keys[index].Name;
            RemoveKey(index);
            return AddKey(texture, time, name);
        }

        public void UpdateKeyName(int index, string name)
        {
            Keys[index] = new TextureKey(Keys[index].Texture2D, Keys[index].Time, name);
        }

        public void UpdateKeyTexture(int index, Texture2D texture)
        {
            Keys[index] = new TextureKey(texture, Keys[index].Time, Keys[index].Name);
        }

        public int UpdateKey(int index, Texture2D texture, float time, string name = "")
        {
            UpdateKeyTexture(index, texture);
            UpdateKeyName(index, name);
            return UpdateKeyTime(index, time);
        }

        public override Color Evaluate(float time)
        {
            if (Keys.Count == 0)
                return Color.white;

            int keyLeft = 0, keyRight = NumKey - 1;

            for (int i = 0; i < NumKey; ++i)
            {
                if (Keys[i].Time > time && Keys[i].Time < time)
                    continue;

                if (Keys[i].Time <= time)
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
                    return Color.white;// Keys[keyRight].Texture2D;
            }
            //Keys[keyLeft].Texture2D.get
            return Color.white;//Texture2D.Lerp(Keys[keyLeft].Texture2D, Keys[keyRight].Texture2D, Mathf.InverseLerp(Keys[keyLeft].Time, Keys[keyRight].Time, time));
        }

        public override TextureKey GetKey(int index)
        {
            return Keys[index];
        }

        public override Texture2D GetTexture(int width)
        {
            Color[] colors = new Color[width];
            for (int x = 0; x < width; x++)
                colors[x] = Evaluate((float)x / (width - 1));

            return UtilityClass.TextureFromColors(colors, width, 1);
        }

        public override int UpdateKey(int index, TextureKey Key)
        {
            UpdateKeyTexture(index, Key.Texture2D);
            UpdateKeyName(index, Key.Name);
            return UpdateKeyTime(index, Key.Time);
        }
    }

    [System.Serializable]
    public abstract class CustomGradient<T>
    {
        public BlendMode blendMode;

        [SerializeField] protected List<T> Keys = new List<T>();
        public int NumKey { get { return Keys.Count; } }

        public abstract int AddKey(T key);
        public abstract void RemoveKey(int index);
        public abstract int UpdateKey(int index, T Key);
        public abstract Color Evaluate(float time);
        public abstract T GetKey(int index);
        public abstract Texture2D GetTexture(int width);
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

    [System.Serializable]
    public struct TextureKey
    {
        [SerializeField] string name;
        [SerializeField] Texture2D texture;
        [SerializeField] float time;

        public TextureKey(Texture2D texture, float time, string name = "")
        {
            this.name = name;
            this.texture = texture;
            this.time = time;
        }

        public Texture2D Texture2D { get { return texture; } }
        public float Time { get { return time; } }
        public string Name { get { return name; } }
    }
}