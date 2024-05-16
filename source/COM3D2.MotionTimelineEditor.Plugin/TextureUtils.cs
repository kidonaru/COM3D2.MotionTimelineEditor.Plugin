using System;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static class TextureUtils
    {
        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(array);
            return texture2D;
        }

        public static void ClearTexture(Texture2D texture, Color color)
        {
            var pixels = new Color[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
        }

        public static void DrawLineTexture(
            Texture2D texture,
            int x0,
            int y0,
            int x1,
            int y1,
            Color color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2;
            int e2;

            while (true)
            {
                if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
                {
                    texture.SetPixel(x0, y0, color);
                }
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }

        public static void DrawCircleLineTexture(
            Texture2D texture,
            float radius,
            int segments,
            Color lineColor)
        {
            var width = texture.width;
            var height = texture.height;
            var centerX = width / 2;
            var centerY = height / 2;

            double angleStep = 2 * Math.PI / segments;
            int x0 = centerX + (int) radius;
            int y0 = centerY;

            for (int i = 1; i <= segments; i++)
            {
                int x1 = centerX + (int)(radius * Math.Cos(i * angleStep));
                int y1 = centerY + (int)(radius * Math.Sin(i * angleStep));

                DrawLineTexture(texture, x0, y0, x1, y1, lineColor);

                x0 = x1;
                y0 = y1;
            }

            texture.Apply();
        }

        // ひし形のテクスチャを作成
        public static Texture2D CreateDiamondTexture(
            int size,
            Color color)
        {
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];
            var bgColor = new Color(0, 0, 0, 0);
            int halfSize = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distanceX = Math.Abs(x - halfSize);
                    int distanceY = Math.Abs(y - halfSize);
                    if (distanceX + distanceY <= halfSize)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = bgColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateCircleTexture(
            int size,
            Color color)
        {
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];
            var bgColor = new Color(0, 0, 0, 0);
            int halfSize = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int distanceX = x - halfSize;
                    int distanceY = y - halfSize;
                    if (distanceX * distanceX + distanceY * distanceY <= halfSize * halfSize)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = bgColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}