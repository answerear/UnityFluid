using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Framework
{
    public static class Log
    {
        private const string mSpliter = "$";
        private const int TrimStrBufferSize = 4096;
        private const int StrBufferInitSize = 256;
        //private static StringBuilder StrBuffer = new StringBuilder(StrBufferInitSize);

        public static Action<int, string> PrintLog { get; set; }

        public enum LEVEL
        {
            OFF,
            Error,
            EVENT,
            Info,
            Debug,
            Warning,
            MAX,
        }

        private static ThreadLocal<StringBuilder> StrBufferLocal = new ThreadLocal<StringBuilder>(() => { return new StringBuilder(StrBufferInitSize); }, false);
        private static StringBuilder StrBuffer
        {
            get
            {
                return StrBufferLocal.Value;
            }
        }

        public static void Debug<T1>(string tag, T1 arg1)
        {
            WriteLog(LEVEL.Debug, tag, arg1);
        }

        public static void Debug<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LEVEL.Debug, tag, arg1, arg2);
        }

        public static void Debug<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LEVEL.Debug, tag, arg1, arg2, arg3);
        }

        public static void Debug<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LEVEL.Debug, tag, arg1, arg2, arg3, arg4);
        }

        public static void Info<T1>(string tag, T1 arg1)
        {
            WriteLog(LEVEL.Info, tag, arg1);
        }

        public static void Info<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LEVEL.Info, tag, arg1, arg2);
        }

        public static void Info<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LEVEL.Info, tag, arg1, arg2, arg3);
        }

        public static void Info<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LEVEL.Info, tag, arg1, arg2, arg3);
        }

        public static void Warning<T1>(string tag, T1 arg1)
        {
            WriteLog(LEVEL.Warning, tag, arg1);
        }

        public static void Warning<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LEVEL.Warning, tag, arg1, arg2);
        }

        public static void Warning<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LEVEL.Warning, tag, arg1, arg2, arg3);
        }

        public static void Warning<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LEVEL.Warning, tag, arg1, arg2, arg3);
        }

        public static void Error<T1>(string tag, T1 arg1)
        {
            WriteLog(LEVEL.Error, tag, arg1);
        }

        public static void Error<T1, T2>(string tag, T1 arg1, T2 arg2)
        {
            WriteLog(LEVEL.Error, tag, arg1, arg2);
        }

        public static void Error<T1, T2, T3>(string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            WriteLog(LEVEL.Error, tag, arg1, arg2, arg3);
        }

        public static void Error<T1, T2, T3, T4>(string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            WriteLog(LEVEL.Error, tag, arg1, arg2, arg3, arg4);
        }

        private static void TrimStringBuffer()
        {
            if (StrBuffer.Length != 0)
            {
                StrBuffer.Length = 0;
            }
            if (StrBuffer.Length >= TrimStrBufferSize)
            {
                StrBuffer.Capacity = StrBufferInitSize;
            }
        }

        private static void AppendErrorStack(LEVEL level, string tag)
        {
            if (level == LEVEL.Error)
            {
                System.Diagnostics.StackTrace pStackTrace = new System.Diagnostics.StackTrace(3, true);
                StrBuffer.Append(":");
                StrBuffer.Append(pStackTrace);
            }
        }

        private static void WriteLog<T1>(LEVEL level, string tag, T1 arg1)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            AppendErrorStack(level, tag);

            int lvl = (int)level;
            PrintLog(lvl, StrBuffer.ToString());
        }

        private static void WriteLog<T1, T2>(LEVEL level, string tag, T1 arg1, T2 arg2)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            AppendErrorStack(level, tag);

            int lvl = (int)level;
            PrintLog(lvl, StrBuffer.ToString());
        }

        private static void WriteLog<T1, T2, T3>(LEVEL level, string tag, T1 arg1, T2 arg2, T3 arg3)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            StrBuffer.Append(arg3);
            AppendErrorStack(level, tag);

            int lvl = (int)level;
            PrintLog(lvl, StrBuffer.ToString());
        }

        private static void WriteLog<T1, T2, T3, T4>(LEVEL level, string tag, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            TrimStringBuffer();

            StrBuffer.Append(tag);
            StrBuffer.Append(mSpliter);
            StrBuffer.Append(arg1);
            StrBuffer.Append(arg2);
            StrBuffer.Append(arg3);
            StrBuffer.Append(arg4);
            AppendErrorStack(level, tag);

            int lvl = (int)level;
            PrintLog(lvl, StrBuffer.ToString());
        }
    }
}
