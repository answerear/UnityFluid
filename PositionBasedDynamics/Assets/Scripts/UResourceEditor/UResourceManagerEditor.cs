#define SHOW_DETAIL_RES_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UResource;
using System;
using System.IO;
using Framework;

namespace UResourceEditor
{
    public class UResourceManagerEditor : UResourceManagerBase
    {
        public override UnityEngine.Object LoadAssetSync(string group, string path, string name, Type type, string abName = null)
        {
            UnityEngine.Object obj = null;
#if UNITY_EDITOR
            m_StrBuilder.Length = 0;
            m_StrBuilder.Append("Assets/");
            m_StrBuilder.Append(path);
            m_StrBuilder.Append("/");
            m_StrBuilder.Append(name);

            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(m_StrBuilder.ToString(), type);
            if (obj == null)
            {
                m_StrBuilder.Append(".prefab");
                obj = UnityEditor.AssetDatabase.LoadAssetAtPath(m_StrBuilder.ToString(), type);
            }

#if SHOW_DETAIL_RES_LOG
            Log.Debug(LOG_TAG, "LoadAssetSync LoadAsset ", name, " succ");
#endif

#endif
            return obj;
        }

        public override bool TryGetTexture(string group, string path, string name, out Sprite spr)
        {
            bool ret = false;
            spr = null;
#if UNITY_EDITOR
            m_StrBuilder.Length = 0;
            m_StrBuilder.Append("/");
            m_StrBuilder.Append(path);
            m_StrBuilder.Append("/");
            m_StrBuilder.Append(name);

            string fullpath = Application.dataPath + m_StrBuilder.ToString();

            if (File.Exists(fullpath))
            {
                ret = true;
                m_StrBuilder.Insert(0, "Assets");
                // 文件存在，顺便加载
                spr = UnityEditor.AssetDatabase.LoadAssetAtPath(m_StrBuilder.ToString(), typeof(Sprite)) as Sprite;
            }
#endif
            return ret;
        }

        public override bool UnloadAssetBundle(string group, string path, bool unloadAllLoadedObjects)
        {
            return true;
        }
    }
}