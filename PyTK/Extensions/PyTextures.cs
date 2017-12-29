﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using System;
using System.Collections.Generic;

namespace PyTK.Extensions
{
    public static class PyTextures
    {
        /* Basics */

        public static Texture2D clone(this Texture2D t)
        {
            Texture2D clone = new Texture2D(t.GraphicsDevice, t.Width, t.Height);
            Color[] data = new Color[t.Width * t.Height];
            t.GetData(data);
            clone.SetData(data);
            return clone;
        }

        public static Color clone(this Color t)
        {
            return new Color(t.ToVector4());
        }

        public static int getDistanceTo(this Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

     
        /* Manipulation */

        public static Texture2D changeColor(this Texture2D t, ColorManipulation manipulation)
        {
            Color[] colorData = new Color[t.Width * t.Height];
            t.GetData(colorData);
            Texture2D newTexture = new Texture2D(t.GraphicsDevice, t.Width, t.Height);
            
            for (int x = 0; x < t.Width; x++)
                for (int y = 0; y < t.Height; y++)
                    colorData[x * t.Height + y] = changeColor(colorData[x * t.Height + y], manipulation);

            t.SetData(colorData);

            return t;
        }

        public static Texture2D applyPalette(this Texture2D t, List<Color> palette)
        {
            ColorManipulation manipulation = new ColorManipulation(palette);
            return t.changeColor(manipulation);
        }

        public static Texture2D setSaturation(this Texture2D t, float saturation)
        {
            ColorManipulation manipulation = new ColorManipulation(saturation);
            return t.changeColor(manipulation);
        }

        public static Texture2D setLight(this Texture2D t, float light)
        {
            ColorManipulation manipulation = new ColorManipulation(100,light);
            return t.changeColor(manipulation);
        }

        public static Color changeColor(this Color t, ColorManipulation manipulation)
        {
            t = t.setLight(manipulation.light);
            t = t.setSaturation(manipulation.saturation);
            if (manipulation.palette.Count > 0)
                t = t.applyPalette(manipulation.palette);
            return t;
        }

        public static Color multiplyWith(this Color color1, Color color2)
        {
            color1.R = (byte)MathHelper.Min(((color1.R * color2.R) / 255), 255);
            color1.G = (byte)MathHelper.Min(((color1.G * color2.G) / 255), 255);
            color1.B = (byte)MathHelper.Min(((color1.B * color2.B) / 255), 255);
            color1.A = color1.A;

            return color1;
        }

        public static Color setSaturation(this Color t, float saturation, Vector3? saturationMultiplier = null)
        {
            Vector3 m = saturationMultiplier.HasValue ? saturationMultiplier.Value : new Vector3(0.2125f, 0.7154f, 0.0721f);
            float l = m.X * t.R + m.Y * t.G + m.Z * t.B;
            float s = 1f - (saturation / 100);

            float newR = t.R;
            float newG = t.G;
            float newB = t.B;

            if (s != 0)
            {
                newR = newR + s * (l - newR);
                newG = newG + s * (l - newG);
                newB = newB + s * (l - newB);
            }

            t.R = (byte)MathHelper.Min(newR, 255);
            t.G = (byte)MathHelper.Min(newG, 255);
            t.B = (byte)MathHelper.Min(newB, 255);
            /*
            t.R = (byte)Math.Min(t.R + (s * (l - t.R)), 255);
            t.G = (byte)Math.Min(t.G + (s * (l - t.G)), 255);
            t.B = (byte)Math.Min(t.B + (s * (l - t.B)), 255);*/
            return t;
        }

        public static Color setLight(this Color t, float light)
        {
            float l = light / 100;
            t.R = (byte) Math.Min(t.R * l,255);
            t.G = (byte)Math.Min(t.G * l, 255);
            t.B = (byte)Math.Min(t.B * l, 255);

            return t;
        }

        public static Color applyPalette(this Color current, List<Color> palette)
        {
            int index = -1;
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                int distance = current.getDistanceTo(palette[i]);
                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return palette[index];
        }

    }
}
