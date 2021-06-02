using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFramework;
using Framework;

namespace UPPhysXDemo
{
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot instance = null;

        private ResourceManager resourceMgr = null;

        private void Awake()
        {
            instance = this;

            Framework.Log.PrintLog = Debug.WriteLog;

            FrameworkSystem.CreateInstance();
            FrameworkSystem.Instance.Startup(0);

            resourceMgr = ResourceManager.CreateInstance();

#if UNITY_EDITOR
            resourceMgr.ResourceMgr = new UResourceEditor.UResourceManagerEditor();
#else
            resourceMgr.ResourceMgr = new UResourceRuntime.UResourceManagerRuntime();
#endif

            ResourceManager.Instance.Init("PositionBasedDynamics");

            GameObject fluid = ResourceManager.Instance.LoadPrefabSync("PBD", "Prefabs", "FluidDemo");
            fluid.transform.SetParent(transform, false);
        }

        private void Update()
        {
            FrameworkSystem.Instance.Update();
        }

        private void FixedUpdate()
        {
            
        }

        private void OnDestroy()
        {
            ResourceManager.DestroyInstance();

            FrameworkSystem.Instance.Shutdown();
            FrameworkSystem.DestroyInstance();

            instance = null;
        }
    }
}
