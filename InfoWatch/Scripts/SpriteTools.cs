using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace InfoWatch.Scripts
{
    internal class SpriteTools
    {
        /// <summary>
        /// Make a sprite from a byte array
        /// </summary>
        /// <param name="byteArray">Array of bytes of an appropriate image.</param>
        /// <param name="name">Name of the generated texture.</param>
        /// <param name="width">Width of the texture and sprite.</param>
        /// <param name="height">Height of the texture and sprite.</param>
        /// <returns>A sprite created using the bytes passed in.</returns>
        public static Sprite CreateSpriteFromByteArray(byte[] byteArray, string name, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, name = name };
            tex.LoadImage(byteArray);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.zero);
        }
    }
}
