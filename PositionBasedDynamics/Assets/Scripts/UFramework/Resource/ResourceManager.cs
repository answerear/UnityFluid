using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UResource;

namespace UFramework
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public UResourceManagerBase ResourceMgr { set { mResourceMgr = value; } get { return mResourceMgr; } }

        private UResourceManagerBase mResourceMgr = null;

        #region 接口
        public void Init(string name)
        {
            mResourceMgr.Init(name);
        }

        public bool ResetGroup(string group)
        {
            return mResourceMgr.ResetGroup(group);
        }

        public GameObject LoadPrefabSync(string group, string path, string name, string abName = null)
        {
            return mResourceMgr.LoadPrefabSync(group, path, name, typeof(GameObject), abName) as GameObject;
        }

        public UnityEngine.Object LoadAssetSync(string group, string path, string name, Type type, string abName = null)
        {
            return mResourceMgr.LoadAssetSync(group, path, name, type, abName);
        }

        public bool TryGetTexture(string group, string path, string name, out Sprite spr)
        {
            return mResourceMgr.TryGetTexture(group, path, name, out spr);
        }

        public bool UnloadAssetBundle(string group, string path, bool unloadAllLoadedObjects)
        {
            return mResourceMgr.UnloadAssetBundle(group, path, unloadAllLoadedObjects);
        }

        public bool IsAssetbundleLoaded(string group, string path, out AssetBundle ab)
        {
            return mResourceMgr.IsAssetbundleLoaded(group, path, out ab);
        }

        public bool IsAbPathExist(string group, string path)
        {
            return mResourceMgr.IsAbPathExist(group, path);
        }
        #endregion
    }
}

