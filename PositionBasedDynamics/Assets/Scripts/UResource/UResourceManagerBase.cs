//#define SHOW_DETAIL_RES_LOG
using Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UResource
{
    public class AssetBundleItem
    {
        public AssetBundle Bundle;                      // Asset Bundle 对象
        public int RefCount;                            // 总的引用计数
        public Dictionary<string, int> GrpRefCount;     // 虚拟组引用计数

        public AssetBundleItem(AssetBundle assetBundle)
        {
            Bundle = assetBundle;
            RefCount = 1;
            GrpRefCount = new Dictionary<string, int>();
        }
    }

    public class GroupItem
    {
        public Dictionary<string, string> AssetBundlesMap;          // 用于索引访问 Asset Bundle 的映射表
        public Dictionary<string, AssetBundleItem> AssetBundles;    // Asset Bundle 对象，包括依赖包

        public GroupItem()
        {
            AssetBundlesMap = new Dictionary<string, string>();
            AssetBundles = new Dictionary<string, AssetBundleItem>();
        }
    }

    

    public abstract class UResourceManagerBase : MonoBehaviour
    {
        protected static readonly string LOG_TAG = "UResourceManager";

        #region 成员
        protected UFileCrypt mFileCrypt;
        protected string mSetName;
        protected StringBuilder m_StrBuilder;
        protected Dictionary<string, GroupItem> m_Groups;
        protected AssetBundleManifest m_Manifest;                     // 资源包的 manifest 对象
        protected Dictionary<string, AssetBundleItem> m_AssetBundles; // Asset Bundle 对象
        protected Dictionary<string, Texture2D> m_Textures; // 创建过的Texture map
        #endregion

        #region 构造和析构
        public UResourceManagerBase()
        {
            mFileCrypt = new UFileCrypt();
            mSetName = "";
            m_StrBuilder = new StringBuilder();
            m_Groups = new Dictionary<string, GroupItem>();
            m_Manifest = null;
            m_AssetBundles = new Dictionary<string, AssetBundleItem>();
            m_Textures = new Dictionary<string, Texture2D>();
        }

        ~UResourceManagerBase()
        {

        }
        #endregion

        #region 公有接口
        public void Init(string setName)
        {
            mSetName = setName;
        }

        public void UnloadAllAssetBundles()
        {
            foreach (var group in m_Groups.Keys)
            {
                ResetGroup(group);
            }

            m_Groups.Clear();
        }

        /// <summary>
        /// 重置分组，方便更新资源后重新加载所有资源
        /// </summary>
        /// <param name="group">资源分组名称</param>
        /// <returns>调用成功返回true</returns>
        public bool ResetGroup(string group)
        {
            bool ret = false;

            do
            {
                GroupItem groupItem = null;
                ret = m_Groups.TryGetValue(group, out groupItem);
                if (!ret)
                {
                    break;
                }

                // 卸载所有 Asset Bundles
                var enumBundles = groupItem.AssetBundles.GetEnumerator();
                while (enumBundles.MoveNext())
                {
                    var item = enumBundles.Current.Value;
                    string bundleName = item.Bundle.name;
                    ret = ret && UnloadAssetBundle(group, ref groupItem, bundleName, false, true);
                }

                // 清除信息
                groupItem.AssetBundles.Clear();

                ret = true;
            } while (false);
            //Log.Debug("AssetBunlde", "ResetGroup: ", group);
            //DebugAssetBunldes();

            return ret;
        }

        /// <summary>
        /// 同步加载Prefab资源
        /// </summary>
        /// <param name="group">资源分组名称</param>
        /// <param name="path">资源路径，主要是对应AssetBundlesConfig.xml里面的Folder标签对应设置的名称</param>
        /// <param name="name">Prefab资源名称</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回加载到的Prefab资源对象</returns>
        public UnityEngine.Object LoadPrefabSync(string group, string path, string name, Type type, string abName = null)
        {
            UnityEngine.Object obj = null;
            //int id = HLProfiler.StartProfiler();
            obj = LoadAssetSync(group, path, name, type, abName);
            //HLProfiler.StopProfiler(id, string.Format("LoadAssetSync {0} {1} {2} Time Cost ", group, path, name));
            if (obj != null)
            {
#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "LoadPrefabSync Instantiate prefab ", name);
#endif
                //id = HLProfiler.StartProfiler();
                obj = UnityEngine.Object.Instantiate(obj);
#if DYNAMIC_RESTORE_MONOBEHAVIOUR && !EDITOR_AB
                GameObject go = obj as GameObject;
                if (go != null)
                {
                    PrefabsCSTools.RebindMonoBehaviour(go, name);
                }
#endif
                //HLProfiler.StopProfiler(id, string.Format("Instantiate {0} {1} {2} Time Cost ", group, path, name));
#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "LoadPrefabSync Instantiate prefab ", name, " OK");
#endif
                return obj;
            }
            return obj;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="group">资源分组名称</param>
        /// <param name="path">资源路径，主要是对应AssetBundlesConfig.xml里面的Folder标签对应设置的名称</param>
        /// <param name="name">Prefab资源名称</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回加载到的Asset资源对象</returns>
        public abstract UnityEngine.Object LoadAssetSync(string group, string path, string name, Type type, string abName = null);

        public abstract bool TryGetTexture(string group, string path, string name, out Sprite spr);

        public abstract bool UnloadAssetBundle(string group, string path, bool unloadAllLoadedObjects);

        public AssetBundle CreateABFromEncryptFile(string fileName)
        {
            try
            {
#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "GetDecryptDataFromFile ", fileName);
#endif
                // HLFileCrypt cryptor = new HLFileCrypt();
                // byte[] byteFileData = cryptor.GetDecryptDataFromFile(fileName);
//                 int id = HLProfiler.StartProfiler();
                int len = 0;
                byte[] byteFileData = mFileCrypt.DecryptFileNew(fileName, ref len);
//                 HLProfiler.StopProfiler(id, "DecryptFileNew: " + fileName, true);

#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "GetDecryptDataFromFile ", fileName, " success");
#endif
                if (byteFileData != null)
                {
//                     id = HLProfiler.StartProfiler();
                    Log.Debug(LOG_TAG, "LoadFromMemory ", fileName, " Start " + byteFileData.Length);
                    AssetBundle ab = AssetBundle.LoadFromMemory(byteFileData);
//                     HLProfiler.StopProfiler(id, "LoadFromMemory: " + fileName, true);
#if SHOW_DETAIL_RES_LOG
                    Log.Debug(LOG_TAG, "LoadFromMemory ", fileName, " End");
#endif
                    return ab;
                }
                else
                {
                    Log.Debug(LOG_TAG, "LoadFromMemory ", fileName, " byteFileData Null");
                }
            }
            catch (Exception e)
            {
                Log.Error("", "CreateABFromEncryptFile(): " + e.ToString());
            }
            Log.Info(LOG_TAG, "GetDecryptDataFromFile Failed End ", fileName);
            return null;
        }
        #endregion

        #region 引擎回调
        private void OnDestroy()
        {
            UnloadAllAssetBundles();
        }

        #endregion

        #region 私有方法
        protected bool GetGroupItem(string group, out GroupItem groupItem)
        {
            return m_Groups.TryGetValue(group, out groupItem);
        }

        protected bool LoadAssetBundleManifest()
        {
            bool ret = false;

            do
            {
                m_StrBuilder.Length = 0;
                m_StrBuilder.Append(UPathUtils.Instance.AssetBundlePath);
                m_StrBuilder.Append("/");
                m_StrBuilder.Append(mSetName);
                m_StrBuilder.Append("/");
                m_StrBuilder.Append(mSetName);

                var bundle = CreateABFromEncryptFile(m_StrBuilder.ToString());

                if (bundle == null)
                {
                    Log.Error(LOG_TAG, "Load asset bundle manifest failed");
                    break;
                }

                // 加载AssetBundleManifest
                m_Manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                bundle.Unload(false);

                // 加载路径和AssetBundle映射表
                ret = LoadAssetBundlesMapFile();
                if (!ret)
                {
                    break;
                }

                ret = true;
            } while (false);

            return ret;
        }

        protected bool LoadAssetBundlesMapFile(string strABPath = null)
        {
            bool ret = false;
            if (strABPath == null)
            {
                strABPath = UPathUtils.Instance.AssetBundlePath;
            }

            do
            {
                m_StrBuilder.Length = 0;
                m_StrBuilder.Append(strABPath);
                m_StrBuilder.Append("/");
                m_StrBuilder.Append(mSetName);
                m_StrBuilder.Append("/");
                m_StrBuilder.Append(mSetName);
                m_StrBuilder.Append(".map");
                int len = 0;
                byte[] bytes = mFileCrypt.DecryptFileNew(m_StrBuilder.ToString(), ref len);

                // HLFileCrypt cryptor = new HLFileCrypt();
                // byte[] bytes = cryptor.GetDecryptDataFromFile(m_StrBuilder.ToString());
                if (bytes == null || bytes.Length == 0)
                {
                    Log.Error(LOG_TAG, "Load asset bundles map file failed !");
                    break;
                }

                GroupItem group = null;

                string text = Encoding.UTF8.GetString(bytes, 0, len);
                StringReader reader = new StringReader(text);
                string line = null;
                int cnt = 0;
                while (true)
                {
                    line = reader.ReadLine();

                    if (line == null || line == "" || line[0] == '\0')
                    {
                        break;
                    }

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // Group Name
                        string name = line.Substring(1, line.Length - 2);
                        group = new GroupItem();
                        m_Groups.Add(name, group);
                    }
                    else
                    {
                        // Group Items
                        string[] texts = line.Split('=');

                        if (texts.Length != 2)
                        {
                            Log.Error(LOG_TAG, "Map file ", group, " format error !!!" + line);
                            ret = false;
                            break;
                        }

                        if (group != null)
                        {
                            group.AssetBundlesMap.Add(texts[0], texts[1]);
                            cnt += 1;
                        }
                    }

                    ret = true;
                }
                Log.Info("LoadAssetBundlesMapFile", " Count: " + cnt);
                if (!ret)
                {
                    break;
                }
            } while (false);

            return ret;
        }

        protected bool LoadAssetBundleSync(string group, ref GroupItem groupItem, string name, bool isRealName, out AssetBundle bundle)
        {
            if (group == null || groupItem == null || name == null)
            {
                bundle = null;
                return false;
            }

            bool ret = true;

            bundle = null;

            do
            {
                string bundleName = null;
                if (isRealName)
                {
                    bundleName = name;
                }
                else
                {
                    bundleName = GetAssetBundleName(ref groupItem, name);
                }

                if (bundleName == null)
                {
                    bundle = null;
                    return false;
                }

#if SHOW_DETAIL_RES_LOG
                Log.Debug(LOG_TAG, "LoadAssetBundleSync real bundle name ", bundleName);
#endif

                AssetBundleItem item = null;

                if (m_AssetBundles.TryGetValue(bundleName, out item))
                {
                    // 已经有缓存过的 Asset Bundle ，增加引用计数
                    bundle = item.Bundle;
                    //item.RefCount++;
#if SHOW_DETAIL_RES_LOG
                    Log.Debug(LOG_TAG, "LoadAssetBundleSync bundle already loaded !!!");
#endif

                    // 查当前 group 里面是否存在，如果不存在，在当前 group 里面放置一份
                    AssetBundleItem bundlItem = null;
                    if (!groupItem.AssetBundles.TryGetValue(bundleName, out bundlItem))
                    {
                        groupItem.AssetBundles.Add(bundleName, item);
                        if (!item.GrpRefCount.ContainsKey(group))
                        {
                            item.GrpRefCount.Add(group, 1);
                        }
                    }

                    // 把依赖都加进去
                    ret = LoadDependencies(bundleName, group, ref groupItem);
                }
                else
                {
#if SHOW_DETAIL_RES_LOG
                    Log.Debug(LOG_TAG, "LoadAssetBundleSync ", bundleName);
#endif
                    // 没有缓存，加载吧，这里默认本地肯定存在的
                    string path = GetAssetBundlePath(group, bundleName);

                    AssetBundle assetBundle = CreateABFromEncryptFile(path); //AssetBundle.LoadFromFile(abPath);

                    if (assetBundle != null)
                    {
                        item = new AssetBundleItem(assetBundle);
                        item.GrpRefCount.Add(group, 1);
                        m_AssetBundles.Add(bundleName, item);

                        groupItem.AssetBundles.Add(bundleName, item);
                    }
                    else
                    {
                        ret = false;
                        Log.Error(LOG_TAG, "AssetBundle.LoadFromFile failed ", path);
                        break;
                    }

                    Log.Debug(LOG_TAG, "LoadAssetBundleSync ", bundleName, " success");

                    bundle = assetBundle;

                    // 加载所有依赖资源
                    ret = LoadDependencies(bundleName, group, ref groupItem);
                }
            } while (false);

            return ret;
        }

        public bool IsAssetbundleLoaded(string group, string path, out AssetBundle ab)
        {
            GroupItem gItem;
            ab = null;
            if (GetGroupItem(group, out gItem))
            {
                string bundleName = GetAssetBundleName(ref gItem, path);
                if (bundleName != null)
                {
                    AssetBundleItem abItem = null;
                    if (m_AssetBundles.TryGetValue(bundleName, out abItem))
                    {
                        ab = abItem.Bundle;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsAbPathExist(string group, string path)
        {
            if (m_Manifest == null)
            {
                // 加载 manifest 先
                LoadAssetBundleManifest();
            }
            GroupItem gItem;
            if (GetGroupItem(group, out gItem))
            {
                string bundleName = GetAssetBundleName(ref gItem, path);
                if (bundleName != null)
                {
                    return true;
                }
            }
            else
            {
                Log.Error("IsAssetbundlePathExist", "Group Not Found: " + group);
            }
            return false;
        }

        protected bool LoadDependencies(string bundleName, string group, ref GroupItem groupItem)
        {
            bool ret = true;

            do
            {
                if (m_Manifest != null)
                {
                    var dependencies = m_Manifest.GetAllDependencies(bundleName);
#if SHOW_DETAIL_RES_LOG
                    Log.Debug(LOG_TAG, "LoadAssetBundleSync loading all dependencies for ", bundleName, " Length ", dependencies.Length.ToString());
#endif
                    if (dependencies.Length > 0)
                    {
#if SHOW_DETAIL_RES_LOG
                        Log.Debug(LOG_TAG, "LoadAssetBundleSync loading all dependencies for ", bundleName);
#endif
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            AssetBundleItem item = null;
                            m_AssetBundles.TryGetValue(dependencies[i], out item);
                            int get = 0;
                            if (item != null)
                            {
                                item.GrpRefCount.TryGetValue(group, out get);
                            }
                            if (get == 0)
                            {
#if SHOW_DETAIL_RES_LOG
                                Log.Debug(LOG_TAG, "LoadPrefabSync loading dependency asset bundle ", dependencies[i], " for ", bundleName);
#endif
                                AssetBundle depBundle;

                                ret = ret && LoadAssetBundleSync(group, ref groupItem, dependencies[i], true, out depBundle);
                            }
                            else
                            {
                                ret = true;
                            }
                        }

                        if (!ret)
                        {
                            break;
                        }
                    }
                    else
                    {
                        ret = true;
                    }
                }
                else
                {
                    Log.Error(LOG_TAG, "LoadAssetBundleSync manifest null");
                }
            } while (false);

            return ret;
        }

        protected string GetAssetBundleName(ref GroupItem item, string path)
        {
            //             string realName = name.Replace("/", "_");
            //             return realName.ToLower();
            string name = null;
            if (item.AssetBundlesMap.TryGetValue(path.ToLower(), out name))
            {

            }
            return name;
        }

        protected string GetAssetBundlePath(string group, string name)
        {
            m_StrBuilder.Length = 0;
            m_StrBuilder.Append(UPathUtils.Instance.AssetBundlePath);
            m_StrBuilder.Append("/");
            //             m_StrBuilder.Append(group);
            //             m_StrBuilder.Append("/");
            m_StrBuilder.Append(mSetName);
            m_StrBuilder.Append("/");
            m_StrBuilder.Append(name);
            return m_StrBuilder.ToString();
        }

        protected bool UnloadAssetBundle(string group, ref GroupItem groupItem, string bundleName, bool bRemove, bool unloadAllLoadedObjects)
        {
            bool ret = false;
            // 获取Asset Bundle
            AssetBundleItem item = null;
            ret = m_AssetBundles.TryGetValue(bundleName, out item);
            do
            {
                if (!ret)
                {
                    break;
                }

                // 查看在本虚拟组里面的引用值
                int grpRefCount = 0;
                if (!item.GrpRefCount.TryGetValue(group, out grpRefCount))
                {
                    break;
                }
                item.GrpRefCount.Remove(group);

                if (item.GrpRefCount.Count == 0)
                {
                    // 删掉全局的引用，直接卸载资源了
                    item.Bundle.Unload(unloadAllLoadedObjects);
                    item.Bundle = null;
                    m_AssetBundles.Remove(bundleName);

                    Log.Debug(LOG_TAG, bundleName, " has been unloaded successfully");
                }

                ret = true;
            } while (false);
            return ret;
        }

        protected string GetRealName(GroupItem groupItem, string path, bool isRealName)
        {
            string bundleName = null;
            if (isRealName)
            {
                bundleName = path;
            }
            else
            {
                bundleName = GetAssetBundleName(ref groupItem, path);
            }
            return bundleName;
        }
        #endregion
    }
}