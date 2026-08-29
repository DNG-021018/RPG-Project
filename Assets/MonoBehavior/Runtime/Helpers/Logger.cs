using UnityEngine;

namespace RPG.Helpers
{
    public class Logger
    {
        Color LogColor = Color.white;
        Color WarningColor = Color.yellow;
        Color ErrorColor = Color.red;

        public void Log<T>(T value, string message)
        {
            Debug.Log($"{value} - <color=#{ColorUtility.ToHtmlStringRGB(LogColor)}>{message}</color>");
        }

        public void LogWarning<T>(T value, string message)
        {
            Debug.LogWarning($"{value} - <color=#{ColorUtility.ToHtmlStringRGB(WarningColor)}>{message}</color>");
        }

        public void LogError<T>(T value, string message)
        {
            Debug.LogError($"{value} - <color=#{ColorUtility.ToHtmlStringRGB(ErrorColor)}>{message}</color>");
        }
    }
}
