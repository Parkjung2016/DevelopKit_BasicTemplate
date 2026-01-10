using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    public static class SkddkkkkDebug
    {
        #region 화면 로그용 내부 MonoBehaviour

        private class FloatingDebug : MonoBehaviour
        {
            public class LogMessage
            {
                public string message;
                public float expireTime;
                public Color color;
            }

            private List<LogMessage> messages = new List<LogMessage>();
            private static FloatingDebug instance;

            public static FloatingDebug Instance
            {
                get
                {
                    if (instance == null)
                    {
                        var go = new GameObject("SkddkkkkFloatingDebug");
                        instance = go.AddComponent<FloatingDebug>();
                        UnityEngine.Object.DontDestroyOnLoad(go);
                    }

                    return instance;
                }
            }

            public void AddMessage(string message, Color? color = null, float duration = 2f)
            {
                messages.Add(new LogMessage
                {
                    message = message,
                    expireTime = Time.time + duration,
                    color = color ?? Color.white
                });
            }

            private void OnGUI()
            {
                GUIStyle style = new GUIStyle();

                style.fontSize = Mathf.RoundToInt(Screen.height * 0.02f);
                style.normal.textColor = Color.white;

                float y = 10f;
                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    var log = messages[i];
                    if (Time.time > log.expireTime)
                    {
                        messages.RemoveAt(i);
                        continue;
                    }

                    style.normal.textColor = log.color;

                    float labelHeight = style.fontSize + 5;
                    GUI.Label(new Rect(10, y, 1000, labelHeight), log.message, style);
                    y += labelHeight + 5;
                }

                style.normal.textColor = Color.white;
            }
        }

        #endregion

        #region 기본 로그

        [Conditional("ENABLE_LOG")]
        public static void Log(object message, UnityEngine.Object context = null, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string msg = BuildMessage(message, prefixNumber, showTimestamp, callerFilePath, callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(msg, null, duration);
            else
                UnityEngine.Debug.Log(msg, context);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(object message, UnityEngine.Object context = null, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string msg = BuildMessage(message, prefixNumber, showTimestamp, callerFilePath, callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(msg, Color.yellow, duration);
            else
                UnityEngine.Debug.LogWarning(msg, context);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(object message, UnityEngine.Object context = null, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string msg = BuildMessage(message, prefixNumber, showTimestamp, callerFilePath, callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(msg, Color.red, duration);
            else
                UnityEngine.Debug.LogError(msg, context);
        }

        #endregion

        #region 색상 로그

        [Conditional("ENABLE_LOG")]
        public static void LogColor(string message, Color color, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string appliedColorMessage = ApplyColors(message, color);
            string output = BuildMessage(appliedColorMessage, prefixNumber, showTimestamp, callerFilePath,
                callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(output, color, duration);
            else
                UnityEngine.Debug.Log(output);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogColorWarning(string message, Color color = default, UnityEngine.Object context = null,
            int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string appliedColorMessage = ApplyColors(message, color);
            string output = BuildMessage(appliedColorMessage, prefixNumber, showTimestamp, callerFilePath,
                callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(output, color == default ? Color.yellow : color, duration);
            else
                UnityEngine.Debug.LogWarning(output, context);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogColorError(string message, Color color = default, UnityEngine.Object context = null, int?
                prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            string appliedColorMessage = ApplyColors(message, color);
            string output = BuildMessage(appliedColorMessage, prefixNumber, showTimestamp, callerFilePath,
                callerMember);
            if (showOnScreen)
                FloatingDebug.Instance.AddMessage(output, color == default ? Color.red : color, duration);
            else
                UnityEngine.Debug.LogError(output, context);
        }

        #endregion

        #region Format 로그

        [Conditional("ENABLE_LOG")]
        public static void LogFormat(UnityEngine.Object context, string format, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "",
            params object[] args)
        {
            string message = string.Format(format, args);
            Log(message, context, prefixNumber, showTimestamp, showOnScreen, duration, callerFilePath,
                callerMember);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "",
            params object[] args)
        {
            string message = string.Format(format, args);
            LogWarning(message, context, prefixNumber, showTimestamp, showOnScreen, duration, callerFilePath,
                callerMember);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogErrorFormat(UnityEngine.Object context, string format, int? prefixNumber = null,
            bool showTimestamp = false, bool showOnScreen = false, float duration = 2f,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "",
            params object[] args)
        {
            string message = string.Format(format, args);
            LogError(message, context, prefixNumber, showTimestamp, showOnScreen, duration, callerFilePath,
                callerMember);
        }

        #endregion

        #region ColorFormat 로그

        [Conditional("ENABLE_LOG")]
        public static void LogColorFormat(UnityEngine.Object context, Color color, string format,
            int? prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false,
            float duration = 2f, [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "", params object[] args)
        {
            string message = string.Format(format, args);
            LogColor(message, color, prefixNumber, showTimestamp, showOnScreen, duration, callerFilePath,
                callerMember);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogColorWarningFormat(UnityEngine.Object context, string format, Color color = default,
            int? prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false,
            float duration = 2f, [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "", params object[] args)
        {
            string message = string.Format(format, args);
            LogColorWarning(message, color, context, prefixNumber, showTimestamp, showOnScreen, duration,
                callerFilePath, callerMember);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogColorErrorFormat(UnityEngine.Object context, Color color, string format,
            int? prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false,
            float duration = 2f, [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "", params object[] args)
        {
            string message = string.Format(format, args);
            LogColorError(message, color, context, prefixNumber, showTimestamp, showOnScreen, duration,
                callerFilePath, callerMember);
        }

        #endregion

        #region Assert

        [Conditional("ENABLE_LOG")]
        public static void Assert(bool condition, string message = "", UnityEngine.Object context = null,
            int? prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false,
            float duration = 2f, [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "")
        {
            if (!condition)
            {
                string msg = BuildMessage(message, prefixNumber, showTimestamp, callerFilePath, callerMember);
                if (showOnScreen)
                    FloatingDebug.Instance.AddMessage(msg, Color.red, duration);
                else
                    UnityEngine.Debug.Assert(false, msg, context);
            }
        }

        [Conditional("ENABLE_LOG")]
        public static void AssertFormat(bool condition, UnityEngine.Object context, string format,
            int? prefixNumber = null, bool showTimestamp = false, bool showOnScreen = false,
            float duration = 2f, [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMember = "", params object[] args)
        {
            if (!condition)
            {
                string message = string.Format(format, args);
                string msg = BuildMessage(message, prefixNumber, showTimestamp, callerFilePath, callerMember);
                if (showOnScreen)
                    FloatingDebug.Instance.AddMessage(msg, Color.red, duration);
                else
                    UnityEngine.Debug.Assert(false, msg, context);
            }
        }

        #endregion

        #region 내부 유틸

        private static string BuildMessage(
            object message,
            int? prefixNumber,
            bool showTimestamp,
            string callerFilePath,
            string callerMember
        )
        {
            var className = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);
            string tag = className;
            // 함수명까지 원하면 ↓
            // string tag = $"{className}.{callerMember}";

            string numberPrefix = prefixNumber.HasValue ? $"#{prefixNumber.Value:000} " : "";
            string tagPrefix = $"[{tag}] ";
            string timestamp = showTimestamp ? $"[{DateTime.Now:HH:mm:ss.fff}] " : "";

            return $"{numberPrefix}{tagPrefix}{timestamp}{message}";
        }


        private static string ApplyColors(string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            string coloredPart = $"<color=#{colorHex}>{text}</color>";

            return coloredPart;
        }

        #endregion
    }
}