using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace UPPhysXDemo
{
    public class Console
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

        private static StreamWriter writer = null;

        public static void Startup()
        {
            string path = Application.persistentDataPath + "/" + "Demo.log";
            FileInfo fi = new FileInfo(path);
            writer = fi.CreateText();
        }

        public static void Shutdown()
        {
            writer.Dispose();
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
            writer.WriteLine(msg);
        }

        static public void WriteLog(int level, string msg)
        {
            PrintLog((LogLevel)level, msg);
        }
    }
}
