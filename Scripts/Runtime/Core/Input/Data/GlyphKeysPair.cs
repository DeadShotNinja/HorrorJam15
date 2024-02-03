using System;
using TMPro;
using UnityEngine;

namespace HJ.Input
{
    [Serializable]
    public sealed class GlyphKeysPair
    {
        public TMP_SpriteGlyph Glyph;
        public string[] MappedKeys;
        public Vector2 Scale = Vector2.one;
    }
}
