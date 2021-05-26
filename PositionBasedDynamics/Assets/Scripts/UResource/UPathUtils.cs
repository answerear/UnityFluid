using Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UResource
{
    public class UPathUtils : Singleton<UPathUtils>
    {
        #region 屬性
        public string DownloadDataPath { get; private set; }
        public string ProtocolDataPath { get; private set; }
        public string AccountDataPath { get; private set; }
        public string CommonDataPath { get; private set; }
        public string AssetBundlePath { get; private set; }
        public string DataPath { get; private set; }
        public string ConfigPath { get; private set; }
        public string TDDataPath { get; private set; }
        public string StreamingAssetsPath { get; private set; }
        public string PersistentDataPath { get; private set; }
        public string AudioPath { get; private set; }
        public string HeadPath { get; private set; }
        #endregion

        public UPathUtils()
        {
            DownloadDataPath = Application.persistentDataPath + "/Downloads";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            AssetBundlePath = Application.streamingAssetsPath + "/AssetBundles";
#else
            AssetBundlePath = Application.persistentDataPath + "/AssetBundles";
#endif
            AccountDataPath = Application.persistentDataPath + "/accounts";

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            DataPath = Application.streamingAssetsPath + "/Data";
#else
            DataPath = Application.persistentDataPath + "/Data";
#endif
            ConfigPath = DataPath + "/Config";
            TDDataPath = DataPath + "/TDData";
            HeadPath = DataPath + "/Head";
            string activityPath = DataPath + "/Activity";
            string goodsPath = DataPath + "/Goods";
            string shopPath = DataPath + "/Shop";
            string mainMeunPath = DataPath + "/MainMenu";
            AudioPath = Application.persistentDataPath + "/Audio";
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            ProtocolDataPath = Application.streamingAssetsPath + "/ProtoTDR";
#else
            ProtocolDataPath = Application.persistentDataPath + "/ProtoTDR";
#endif
            StreamingAssetsPath = Application.streamingAssetsPath;
            PersistentDataPath = Application.persistentDataPath;

            // 创建下载目录
            Directory.CreateDirectory(DownloadDataPath);

            // 创建AB包目录
            Directory.CreateDirectory(AssetBundlePath);
            // 创建数据目录
            Directory.CreateDirectory(DataPath);
            // 创建配置文件目录
            Directory.CreateDirectory(ConfigPath);
            // 创建塔防数据配置文件目录
            Directory.CreateDirectory(TDDataPath);
            Directory.CreateDirectory(TDDataPath + "/Map");
            // 创建头像目录
            Directory.CreateDirectory(HeadPath);
            // 创建活动中心目录
            Directory.CreateDirectory(activityPath);
            // 创建Goods图标目录
            Directory.CreateDirectory(goodsPath);
            // 创建Shop图标目录
            Directory.CreateDirectory(shopPath);
            // 创建MainMenu目录
            Directory.CreateDirectory(mainMeunPath);
            // 创建场次引导图片目录
            Directory.CreateDirectory(DataPath + "/SceneGuide");
            // 创建ProtoTDR目录
            Directory.CreateDirectory(ProtocolDataPath);

            // 创建跟账号无关的公用目录
            CommonDataPath = Application.persistentDataPath + "/Common";
            Directory.CreateDirectory(CommonDataPath);
        }

        public void InitAccountFolder(string account)
        {
            AccountDataPath = Application.persistentDataPath + "/accounts/" + account;
            Directory.CreateDirectory(AccountDataPath);
        }
    }
}
