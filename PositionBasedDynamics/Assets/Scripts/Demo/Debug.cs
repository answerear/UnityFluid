using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UPPhysXDemo
{
    public class Debug
    {
        public enum LogLevel
        {
            OFF,
            Error,
            EVENT,
            Info,
            Debug,
            Warning,
            MAX,
        }

        static private void PrintLog(LogLevel level, string msg)
        {
            // 这个函数给 C# 调用
#if UNITY_EDITOR
            switch (level)
            {
                case LogLevel.Debug: 
                    { 
                        UnityEngine.Debug.Log(msg); 
                    } 
                    break;
                case LogLevel.Info: 
                    { 
                        UnityEngine.Debug.Log(msg); 
                    } 
                    break;
                case LogLevel.Warning: 
                    { 
                        UnityEngine.Debug.LogWarning(msg); 
                    } 
                    break;
                case LogLevel.Error: 
                    { 
                        UnityEngine.Debug.LogError(msg); 
                    } 
                    break;
            }
#endif
        }

        static public void WriteLog(int level, string msg)
        {
            PrintLog((LogLevel)level, msg);
        }
    }
}
