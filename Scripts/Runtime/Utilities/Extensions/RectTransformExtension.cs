using System.Collections.Generic;
using UnityEngine;

namespace HJ.Tools
{
    public static class RectTransformExtension
    {
        public static void SetWidth(this RectTransform rectTransform, float width)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.x = width;
            rectTransform.sizeDelta = size;
        }

        public static void SetHeight(this RectTransform rectTransform, float height)
        {
            Vector2 size = rectTransform.sizeDelta;
            size.y = height;
            rectTransform.sizeDelta = size;
        }

        public static void SetAnchoredX(this RectTransform rectTransform, float x)
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.x = x;
            rectTransform.anchoredPosition = position;
        }

        public static void SetAnchoredY(this RectTransform rectTransform, float y)
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.y = y;
            rectTransform.anchoredPosition = position;
        }

        public static IEnumerable<RectTransform> GetChildTransforms(this RectTransform rectTransform)
        {
            foreach (var item in rectTransform)
            {
                yield return item as RectTransform;
            }
        }
    }
}
