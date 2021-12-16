/// This class is based off functions from Reactor.
/// https://github.com/NuclearPowered/Reactor
/// Usage of this code is under GPL 3.0 license.

using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hazel;


namespace TownOfUs
{
    public static class TempReactorFunctions
    {
 
        public static void Destroy(this UnityEngine.Object obj)
        {
            UnityEngine.Object.Destroy(obj);
        }

        public static T DontDestroy<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.HideAndDontSave;

            return obj.DontDestroyOnLoad();
        }


        public static T DontDestroyOnLoad<T>(this T obj) where T : UnityEngine.Object
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            return obj;
        }

        public static T DontUnload<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return obj;
        }
        /// <summary>
        /// Fully read <paramref name="input"/> stream, can be used as workaround for il2cpp streams.
        /// </summary>
        public static byte[] ReadFully(this Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string ToHtmlStringRGBA(this Color32 color)
        {
            return $"{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
        }

        /// <inheritdoc cref="ToHtmlStringRGBA(UnityEngine.Color32)"/>
        public static string ToHtmlStringRGBA(this Color color)
        {
            return ((Color32)color).ToHtmlStringRGBA();
        }

        public static T LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }

        public static IEnumerable<MethodBase> GetMethods(this Type type, BindingFlags bindingAttr, Type returnType, params Type[] parameterTypes)
        {
            return type.GetMethods(bindingAttr).Where(x => x.ReturnType == returnType && x.GetParameters().Select(x => x.ParameterType).SequenceEqual(parameterTypes));
        }

        public static IEnumerable<MethodBase> GetMethods(this Type type, Type returnType, params Type[] parameterTypes)
        {
            return type.GetMethods(AccessTools.all, returnType, parameterTypes);
        }

        private const float MIN = -50f;
        private const float MAX = 50f;
        private static float ReverseLerp(float t)
        {
            return Mathf.Clamp((t - MIN) / (MAX - MIN), 0f, 1f);
        }

        public static void Write(this MessageWriter writer, Vector2 value)
        {
            var x = (ushort)(ReverseLerp(value.x) * ushort.MaxValue);
            var y = (ushort)(ReverseLerp(value.y) * ushort.MaxValue);

            writer.Write(x);
            writer.Write(y);
        }

        public static Vector2 ReadVector2(this MessageReader reader)
        {
            var x = reader.ReadUInt16() / (float)ushort.MaxValue;
            var y = reader.ReadUInt16() / (float)ushort.MaxValue;

            return new Vector2(Mathf.Lerp(MIN, MAX, x), Mathf.Lerp(MIN, MAX, y));
        }

        /// <summary>
        /// Returns random <typeparamref name="T"/> from <paramref name="input"/>
        /// </summary>
        public static T Random<T>(this IEnumerable<T> input)
        {
            var list = input as IList<T> ?? input.ToList();
            return list.Count == 0 ? default : list[UnityEngine.Random.Range(0, list.Count)];
        }
    }

    public class TownOfUsLog
    {
        public static ManualLogSource Log;
    }
    public static class Coroutines
    {
        public static Coroutine Start(IEnumerator routine)
        {

            return HudManager.Instance.StartCoroutine((Il2CppSystem.Collections.IEnumerator)routine);
        }
    }


    /// <summary>
    /// GUI utilities
    /// </summary>
    public static class GUIExtensions
    {
        /// <summary>
        /// Shortcut for empty texture
        /// </summary>
        public static Texture2D CreateEmptyTexture(int width = 0, int height = 0)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
        }

        /// <summary>
        /// Clamp Rect to screen size
        /// </summary>
        public static Rect ClampScreen(this Rect rect)
        {
            rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);

            return rect;
        }

        /// <summary>
        /// Reset Rect size
        /// </summary>
        public static Rect ResetSize(this Rect rect)
        {
            rect.width = rect.height = 0;

            return rect;
        }

        /// <summary>
        /// Create <see cref="Sprite"/> from <paramref name="tex"/>
        /// </summary>
        public static Sprite CreateSprite(this Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static DefaultControls.Resources StandardResources { get; internal set; } = null!;

        public static GameObject CreateCanvas()
        {
            var gameObject = new GameObject("Canvas");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();

            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referencePixelsPerUnit = 100f;

            gameObject.AddComponent<GraphicRaycaster>();

            return gameObject;
        }

        public static GameObject CreateEventSystem()
        {
            var gameObject = new GameObject("EventSystem");
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
            gameObject.AddComponent<BaseInput>();

            return gameObject;
        }

        public static void SetSize(this RectTransform rectTransform, float width, float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
