#define SHOW_DETAIL_RES_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UResource;
using Framework;
using System;
using System.IO;

namespace UResourceRuntime
{
    public class UResourceManagerStandalone : UResourceManagerBase
    {
        public override UnityEngine.Object LoadAssetSync(string group, string path, string name, Type type, string abName = null)
        {
            UnityEngine.Object obj = null;

            do
            {
                //#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "LoadAssetSync LoadAsset ", name, "......");
                //#endif

                if (m_Manifest == null)
                {
                    // 加载 manifest 先
                    LoadAssetBundleManifest();
                }

                GroupItem groupItem = null;
                if (!GetGroupItem(group, out groupItem))
                {
                    break;
                }

                // 对 path 做一次 调整
                string filename = Path.GetFileNameWithoutExtension(name);
                string tmp = string.Format("{0}/{1}", path, filename);
                if (IsAbPathExist(group, tmp))
                {
                    path = tmp;
                }

                // Log.Error("HLResourceManager：LoadAssetSync", "path = " + path);
                AssetBundle bundle = null;

                do
                {
                    // 这里 只处理了 path 参数， 忽略了 abName
                    bool exist = IsAssetbundleLoaded(group, path, out bundle);
                    if (exist)
                    {
                        break;
                    }
                    if (abName == null)
                    {
                        if (!LoadAssetBundleSync(group, ref groupItem, path, false, out bundle))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!LoadAssetBundleSync(group, ref groupItem, abName, true, out bundle))
                        {
                            break;
                        }
                    }
                } while (false);

#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "LoadAssetSync LoadAsset ", name);
#endif
                if (bundle != null)
                {
                    obj = bundle.LoadAsset(name, type);
                }
            } while (false);

#if SHOW_DETAIL_RES_LOG
            Log.Debug(LOG_TAG, "LoadAssetSync LoadAsset ", name, obj == null ? "fail" : " succ");
#endif

            return obj;
        }

        public override bool TryGetTexture(string group, string path, string name, out Sprite spr)
        {
            bool ret = false;
            spr = null;

            do
            {
                GroupItem groupItem;
                if (!GetGroupItem(group, out groupItem))
                {
                    // 目前假设这里肯定已经Load过这个分组的资源
                    break;
                }

                AssetBundle bundle = null;
                if (!LoadAssetBundleSync(group, ref groupItem, path, false, out bundle))
                {
                    // 没加载到，那就是没有了这个 Asset 了
                    break;
                }

                spr = bundle.LoadAsset(name, typeof(Sprite)) as Sprite;
                if (spr != null)
                {
                    ret = true;
                }
            } while (false);

            return ret;
        }

        public override bool UnloadAssetBundle(string group, string path, bool unloadAllLoadedObjects)
        {
            bool ret = false;

            do
            {
                GroupItem groupItem = null;
                //                 ret = GetAssetBundleManifest(group, out groupItem);
                ret = GetGroupItem(group, out groupItem);

                if (!ret)
                {
                    break;
                }

                string bundleName = GetAssetBundleName(ref groupItem, path);
                //ret = UnloadDependencies(group, ref groupItem, bundleName, true, unloadAllLoadedObjects);
                //if (!ret)
                //{
                //    break;
                //}

                ret = UnloadAssetBundle(group, ref groupItem, bundleName, true, unloadAllLoadedObjects);
                groupItem.AssetBundles.Remove(bundleName);

                if (!ret)
                {
                    break;
                }

                ret = true;
            } while (false);

            return ret;
        }
    }
}