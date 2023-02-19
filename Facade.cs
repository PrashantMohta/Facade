using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Facade
{
    public class Facade : Mod
    {
        internal static Facade Instance;

        internal string DATA_DIR = Satchel.AssemblyUtils.getCurrentDirectory();
        internal string SWAP_FOLDER = "Swap";
        internal List<GameObject> objects = new List<GameObject>();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            CustomKnight.SkinManager.OnSetSkin += SkinManager_OnSetSkin;
            CustomKnight.SwapManager.OnApplySkinUsingProxy += SwapManager_OnApplySkinUsingProxy;
            CustomKnight.DumpManager.BeforeDumpingGameObject += DumpManager_BeforeDumpingGameObject;
        }

        private void SkinManager_OnSetSkin(object sender, EventArgs e)
        {
            foreach(var obj in objects)
            {
                if(obj != null)
                {
                    var customAnimator = obj.GetComponent<CustomAnimator>();
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                    obj.ShowChildrenMatchingPath(customAnimator.animationSet.HideChildren);
                    GameObject.DestroyImmediate(customAnimator.facade);
                    obj.RemoveComponent<CustomAnimator>();
                }
            }
            objects = new List<GameObject>();
        }

        private void DumpManager_BeforeDumpingGameObject(object sender, CustomKnight.DumpEvent e)
        {
            var originalAnimator = e.go.GetComponent<Animator>();
            if (originalAnimator != null)
            {
                var cAnimSet = e.go.GenerateCustomAnimationSet();
                var name = e.go.GetPath(true);
                DumpManager.saveCustomAnimationJson(cAnimSet, e.scene.name, name);
            }
        }

        private void SwapManager_OnApplySkinUsingProxy(object sender, CustomKnight.SwapEvent e)
        {
            if(e.gop.fileType == ".json")
            {
                try
                {
                    SwapManager.loadCustomAnimationSet(e.gop.getObjectPath());
                    e.go.AddCustomAnimator(SwapManager.loadedCustomAnimationSet[e.gop.getObjectPath()]);
                    objects.Add(e.go);
                } catch (Exception ex)
                {
                    this.Log(e.gop.name + " " + ex.ToString());
                }

            }
        }

    }
}