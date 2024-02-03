using System.Linq;
using System.Globalization;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using HJ.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HJ.Tools
{
    public static class GameTools
    {
        public static void AddTo(this System.IDisposable disposable, CompositeDisposable disposables)
        {
            disposables.Add(disposable);
        }

        public static void HandleDisposable(this System.IDisposable disposable)
        {
            if (!GameManager.HasReference)
                throw new System.Exception("Could not handle disposable, because GameManager component reference was not found!");

            GameManager.Instance.Disposables.Add(disposable);
        }

        /// <summary>
        /// Change color alpha (0 is transparent, 1 is opaque).
        /// </summary>
        public static Color Alpha(this Color col, float alpha)
        {
            col.a = alpha;
            return col;
        }

        /// <summary>
        /// Change lightness of the color.
        /// </summary>
        public static Color Lightness(this Color col, float lightness)
        {
            Color.RGBToHSV(col, out var hue, out var saturation, out var _);
            return Color.HSVToRGB(hue, saturation, lightness);
        }

        /// <summary>
        /// Change image color alpha (0 is transparent, 1 is opaque).
        /// </summary>
        public static void Alpha(this Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        
        // /// <summary>
        // /// Set Sound Clip to the Audio Source.
        // /// </summary>
        // public static void SetSoundClip(this AudioSource audioSource, SoundClip soundClip, float volumeMul = 1f, bool play = false)
        // {
        //     if (soundClip == null || soundClip.audioClip == null || audioSource == null) return;
        //
        //     if (audioSource.clip != soundClip.audioClip)
        //         audioSource.clip = soundClip.audioClip;
        //
        //     audioSource.volume = soundClip.volume * volumeMul;
        //
        //     if(play && !audioSource.isPlaying)
        //         audioSource.Play();
        // }
        //
        // /// <summary>
        // /// Play One Shot Sudio Clip in the audio source.
        // /// </summary>
        // public static void PlayOneShotSoundClip(this AudioSource audioSource, SoundClip soundClip, float volumeMul = 1f)
        // {
        //     if (soundClip == null || audioSource == null) 
        //         return;
        //
        //     audioSource.PlayOneShot(soundClip.audioClip, soundClip.volume * volumeMul);
        // }

        /// <summary>
        /// Create new unique Guid.
        /// </summary>
        public static string GetGuid() => System.Guid.NewGuid().ToString("N");

        /// <summary>
        /// Change Cursor States.
        /// </summary>
        public static void ShowCursor(bool locked, bool visible)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = visible;
        }

        /// <summary>
        /// Change the GameObject layer including all children.
        /// </summary>
        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogError("Invalid layer value. Must be between 0 and 31.");
                return;
            }

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static void SetRenderingLayer(this GameObject obj, uint layer, bool set = true)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogError("Invalid layer value. Must be between 0 and 31.");
                return;
            }

            uint layerMask = 1u << (int)layer;

            foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                if (set) renderer.renderingLayerMask |= layerMask;
                else renderer.renderingLayerMask &= ~layerMask;
            }
        }

        /// <summary>
        /// Basic raycast with interact layer checking.
        /// </summary>
        /// <returns>Status whether the object hit by the raycast has an interact layer.</returns>
        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int cullLayers, Layer interactLayer)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cullLayers))
            {
                if (interactLayer.CompareLayer(hit.collider.gameObject))
                {
                    hitInfo = hit;
                    return true;
                }
            }

            hitInfo = default;
            return false;
        }

        /// <summary>
        /// Check that the value-A and value-B are close to within tolerance.
        /// </summary>
        public static bool IsApproximate(float valueA, float valueB, float tollerance)
        {
            return Mathf.Abs(valueA - valueB) < tollerance;
        }

        /// <summary>
        /// Correct the Angle.
        /// </summary>
        public static float FixAngle(this float angle, float min, float max)
        {
            if (angle < min)
                angle += 360F;
            if (angle > max)
                angle -= 360F;

            return angle;
        }

        /// <summary>
        /// Correct the Angle (-180, 180).
        /// </summary>
        public static float FixAngle180(this float angle)
        {
            if (angle < -180F)
                angle += 360F;
            if (angle > 180F)
                angle -= 360F;

            return angle;
        }

        /// <summary>
        /// Correct the Angle (-360, 360).
        /// </summary>
        public static float FixAngle(this float angle)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;

            return angle;
        }

        /// <summary>
        /// Correct the Angle (0 - 360).
        /// </summary>
        public static float FixAngle360(this float angle)
        {
            if (angle < 0)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;

            return angle;
        }

        /// <summary>
        /// Check if value is in vector range.
        /// </summary>
        public static bool InRange(this Vector2 vector, float value, bool equal = false)
        {
            return equal ? value >= vector.x && value <= vector.y :
                value > vector.x && value < vector.y;
        }

        /// <summary>
        /// Check if value is in vector degrees.
        /// </summary>
        public static bool InDegrees(this Vector2 vector, float value, bool equal = false)
        {
            if(vector.x > vector.y)
            {
                return equal ? value >= (vector.x - 360) && value <= vector.y :
                    value > (vector.x - 360) && value < vector.y;
            }
            else
            {
                return equal ? value >= vector.x && value <= vector.y :
                value > vector.x && value < vector.y;
            }
        }

        /// <summary>
        /// Convert string to title case.
        /// </summary>
        public static string ToTitleCase(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Compare Layer with LayerMask.
        /// </summary>
        public static bool CompareLayer(this LayerMask layermask, int layer)
        {
            return layermask == (layermask | (1 << layer));
        }

        /// <summary>
        /// Function to generate a random integer number (No Duplicates).
        /// </summary>
        public static int RandomUnique(int min, int max, int last)
        {
            System.Random rnd = new System.Random();

            if (min + 1 < max)
            {
                return Enumerable.Range(min, max).OrderBy(x => rnd.Next()).Where(x => x != last).Take(1).Single();
            }
            else
            {
                return min;
            }
        }

        /// <summary>
        /// Function to generate a random integer without excluded numbers.
        /// </summary>
        public static int RandomExclude(int min, int max, int[] ex, int maxIterations = 1000)
        {
            int result;
            int iterations = 0;

            do
            {
                if (iterations > maxIterations)
                {
                    result = -1;
                    break;
                }

                result = UnityEngine.Random.Range(min, max);
                iterations++;
            }
            while (ex.Contains(result));

            return result;
        }

        /// <summary>
        /// Function to generate a random unique integer without excluded numbers.
        /// </summary>
        public static int RandomExcludeUnique(int min, int max, int[] ex, int[] current, int maxIterations = 1000)
        {
            return RandomExclude(min, max, ex.Concat(current).ToArray(), maxIterations);
        }

        /// <summary>
        /// Pick a random element from item array.
        /// </summary>
        public static T Random<T>(this T[] items)
        {
            System.Random rnd = new System.Random();
            if(items.Length > 0) return items[rnd.Next(0, items.Length)];
            return default;
        }

        /// <summary>
        /// Get Random Value from Min/Max vector.
        /// </summary>
        public static float Random(this Vector2 vector)
        {
            return UnityEngine.Random.Range(vector.x, vector.y);
        }

        /// <summary>
        /// Get Random Value from Min/Max structure.
        /// </summary>
        public static float Random(this MinMax minMax)
        {
            return UnityEngine.Random.Range(minMax.RealMin, minMax.RealMax);
        }

        /// <summary>
        /// Get Random Value from Min/Max structure.
        /// </summary>
        public static int Random(this MinMaxInt minMax)
        {
            return UnityEngine.Random.Range(minMax.RealMin, minMax.RealMax);
        }

        /// <summary>
        /// Oscillate between the two values at speed.
        /// </summary>
        public static float PingPong(float min, float max, float speed = 1f)
        {
            return Mathf.PingPong(Time.time * speed, max - min) + min;
        }

        /// <summary>
        /// Wrap value between min and max values.
        /// </summary>
        public static int Wrap(int value, int min, int max)
        {
            int newValue = value % max;
            if (newValue < min) newValue = max - 1;
            return newValue;
        }

        /// <summary>
        /// Get closest index from an integer array using a value.
        /// </summary>
        public static int ClosestIndex(this int[] array, int value)
        {
            int closestIndex = 0;
            int minDifference = Mathf.Abs(array[0] - value);

            for (int i = 1; i < array.Length; i++)
            {
                int difference = Mathf.Abs(array[i] - value);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        /// <summary>
        /// Play OneShot Audio Clip 2D.
        /// </summary>
        public static AudioSource PlayOneShot2D(Vector3 position, AudioClip clip, float volume = 1f, string name = "OneShotAudio")
        {
            if(clip == null)
                return null;

            GameObject go = new GameObject(name);
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.clip = clip;
            source.volume = volume;
            source.Play();
            Object.Destroy(go, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
            return source;
        }

        // /// <summary>
        // /// Play OneShot Sound Clip 3D.
        // /// </summary>
        // public static AudioSource PlayOneShot2D(Vector3 position, SoundClip clip, string name = "OneShotAudio")
        // {
        //     if (clip == null || clip.audioClip == null)
        //         return null;
        //
        //     AudioClip audioClip = clip.audioClip;
        //     float volume = clip.volume;
        //     return PlayOneShot2D(position, audioClip, volume, name);
        // }
        //
        // /// <summary>
        // /// Play OneShot Audio Clip 3D.
        // /// </summary>
        // public static AudioSource PlayOneShot3D(Vector3 position, AudioClip clip, float volume = 1f, string name = "OneShotAudio")
        // {
        //     if (clip == null)
        //         return null;
        //
        //     GameObject go = new GameObject(name);
        //     go.transform.position = position;
        //     AudioSource source = go.AddComponent<AudioSource>();
        //     source.spatialBlend = 1f;
        //     source.clip = clip;
        //     source.volume = volume;
        //     source.Play();
        //     Object.Destroy(go, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
        //     return source;
        // }
        //
        // /// <summary>
        // /// Play OneShot Sound Clip 3D.
        // /// </summary>
        // public static AudioSource PlayOneShot3D(Vector3 position, SoundClip clip, string name = "OneShotAudio")
        // {
        //     if (clip == null || clip.audioClip == null)
        //         return null;
        //
        //     AudioClip audioClip = clip.audioClip;
        //     float volume = clip.volume;
        //     return PlayOneShot3D(position, audioClip, volume, name);
        // }

        /// <summary>
        /// Remap range A to range B.
        /// </summary>
        public static float Remap(float minA, float maxA, float minB, float maxB, float t)
        {
            return minB + (t - minA) * (maxB - minB) / (maxA - minA);
        }

        /// <summary>
        /// Replace string part inside two chars
        /// </summary>
        public static string ReplacePart(this string str, char start, char end, string replace)
        {
            int chStart = str.IndexOf(start);
            int chEnd = str.IndexOf(end);
            string old = str.Substring(chStart, chEnd - chStart + 1);
            return str.Replace(old, replace);
        }

        /// <summary>
        /// Replace tag inside two chars (Regex)
        /// </summary>
        public static string RegexReplaceTag(this string str, char start, char end, string tag, string replace)
        {
            Regex regex = new Regex($@"\{start}({tag})\{end}");
            if(regex.Match(str).Success)
                return regex.Replace(str, replace);

            return str;
        }

        /// <summary>
        /// Check if any animator state is being played.
        /// </summary>
        public static bool IsAnyPlaying(this Animator animator)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return (stateInfo.length + 0.1f > stateInfo.normalizedTime || animator.IsInTransition(0)) && !stateInfo.IsName("Default");
        }
    }

    public static class GizmosE
    {
        /// <summary>
        /// Draw arrow using gizmos.
        /// </summary>
        public static void DrawGizmosArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        /// <summary>
        /// Draw wire capsule.
        /// </summary>
        public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
        {
#if UNITY_EDITOR
            // Special case when both points are in the same position
            if (p1 == p2)
            {
                // DrawWireSphere works only in gizmo methods
                Gizmos.DrawWireSphere(p1, radius);
                return;
            }
            using (new Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
            {
                Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
                Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);

                // Check if capsule direction is collinear to Vector.up
                float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
                if (c == 1f || c == -1f)
                {
                    // Fix rotation
                    p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
                }

                // First side
                Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
                Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
                Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
                // Second side
                Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
                Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
                Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
                // Lines
                Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
                Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
                Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
                Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
            }
#endif
        }

        /// <summary>
        /// Draw the label aligned to the center of the position.
        /// </summary>
        public static void DrawCenteredLabel(Vector3 position, string labelText, GUIStyle style = null)
        {
#if UNITY_EDITOR
            if (style == null) style = new GUIStyle(GUI.skin.label);

            GUIContent content = new GUIContent(labelText);
            Vector2 labelSize = style.CalcSize(content);

            // Calculate the offset to center the label
            Vector3 screenPosition = HandleUtility.WorldToGUIPoint(position);
            screenPosition.x -= labelSize.x / 2;
            screenPosition.y -= labelSize.y / 2;
            Vector3 worldPosition = HandleUtility.GUIPointToWorldRay(screenPosition).origin;

            Handles.Label(worldPosition, labelText, style);
#endif
        }

        /// <summary>
        /// Draw disc at the position.
        /// </summary>
        public static void DrawDisc(Vector3 position, float radius, Color outerColor, Color innerColor)
        {
#if UNITY_EDITOR
            Handles.color = innerColor;
            Handles.DrawSolidDisc(position, Vector3.up, radius);
            Handles.color = outerColor;
            Handles.DrawWireDisc(position, Vector3.up, radius);
#endif
        }
    }
}
