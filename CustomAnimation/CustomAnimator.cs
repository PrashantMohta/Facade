using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using static Satchel.TextureUtils;

namespace Facade { 
    internal static class CustomAnimatorExtensions
    {
        public static void SerialiseToFile(this CustomAnimationSet cas,string path)
        {
            var Json = JsonConvert.SerializeObject(cas, Formatting.Indented);
            File.WriteAllText(path, Json);
        }

        public static void DeserialiseFromFile(this CustomAnimationSet cas,string path)
        {
            var Json = File.ReadAllText(path);
            cas.Load(JsonConvert.DeserializeObject<CustomAnimationSet>(Json, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace }));
            var animationRootFolder = Path.GetDirectoryName(path);
            var animationJson = File.ReadAllText(path);
            foreach (var animation in cas.AnimationClips)
            {
                var anim = animation.Value;
                var loadedSprites = new Sprite[anim.frames.Length];
                for (var frame = 0; frame < anim.frames.Length; frame++)
                {
                    var tex = LoadTextureFromFile(Path.Combine(animationRootFolder, anim.frames[frame]));
                    loadedSprites[frame] = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
                }
                Satchel.CustomAnimation.LoadAnimation(animation.Value, loadedSprites);

            }
        }



        public static string GetRelativePath(this GameObject go,GameObject pathFrom, bool useBaseName = false)
        {
            string path = go.GetName(useBaseName);
            GameObject currObj = go;
            while (currObj.transform.parent != null && currObj.transform.parent.gameObject != null && currObj.transform.parent.gameObject != pathFrom)
            {
                currObj = currObj.transform.parent.gameObject;
                path = currObj.GetName(useBaseName) + "/" + path;
            }
            return path;
        }
        internal static CustomAnimationSet GenerateCustomAnimationSet(this GameObject go)
        {
            var originalAnimator = go.GetComponent<Animator>();
            var customAnimationSet  = new CustomAnimationSet();
            customAnimationSet.Name = originalAnimator.runtimeAnimatorController.name;
            foreach (var clip in originalAnimator.runtimeAnimatorController.animationClips) {
                if (!customAnimationSet.HasDefaultAnimation)
                {
                    customAnimationSet.HasDefaultAnimation = true;
                    customAnimationSet.DefaultAnimation = clip.name;
                }
                var newClip = new Satchel.Animation();
                newClip.fps = 10f;
                newClip.loop = true;
                customAnimationSet.AnimationClips[clip.name]=newClip;
            }
            var goList = new List<GameObject>();
            go.FindAllChildren(goList);
            var goPath = go.GetPath(true);
            foreach (var child in goList) {
                customAnimationSet.HideChildren.Add(child.GetRelativePath(go,true));
            }
            customAnimationSet.Enabled = false;
            return customAnimationSet;
        }
        internal static void HideChildrenMatchingPath(this GameObject go, List<string> paths)
        {
            foreach (var childPath in paths)
            {
                var child = go.FindGameObjectInChildren(childPath, true);
                if (child != null)
                {
                    var renderer = child.GetComponent<Renderer>();
                    if(renderer != null)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        internal static void ShowChildrenMatchingPath(this GameObject go, List<string> paths)
        {
            foreach (var childPath in paths)
            {
                var child = go.FindGameObjectInChildren(childPath, true);
                if (child != null)
                {
                    var renderer = child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
            }
        }
        internal static void AddCustomAnimator(this GameObject go, CustomAnimationSet animationSet)
        {
            if (!animationSet.Enabled) {
                return;
            }
            var customAnimator = go.GetComponent<CustomAnimator>();
            if (customAnimator != null) {
                return;
            }
            customAnimator = go.AddComponent<CustomAnimator>();
            customAnimator.enabled = false;

            customAnimator.animationSet = animationSet;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
            go.HideChildrenMatchingPath(customAnimator.animationSet.HideChildren);

            customAnimator.originalAnimator = go.GetComponent<Animator>();
            customAnimator.facade = new GameObject(go.name + "_facade");
            customAnimator.facade.transform.position = new Vector3(0f, 0f, 0f);
            customAnimator.facade.transform.SetParent(go.transform,false);
            customAnimator.facade.transform.localPosition = new Vector3(animationSet.OffsetX, animationSet.OffsetY, animationSet.OffsetZ);
            customAnimator.facade.transform.SetScaleX(animationSet.ScaleX);
            customAnimator.facade.transform.SetScaleX(animationSet.ScaleY);
            customAnimator.controller = customAnimator.facade.GetAddComponent<Satchel.CustomAnimationController>();

            if (customAnimator.originalAnimator != null)
            {
                // customAnimator.originalAnimator.runtimeAnimatorController.animationClips;
                customAnimator.PlayClip(customAnimator.GetCurrentClipOnOriginal());
            }
            else if (customAnimator.animationSet.HasDefaultAnimation)
            {
                customAnimator.PlayClip(customAnimator.animationSet.DefaultAnimation);
            }
            customAnimator.enabled = true;
        }
    } 
    internal class CustomAnimator :MonoBehaviour {
        internal Satchel.CustomAnimationController controller;
        internal CustomAnimationSet animationSet;
        internal GameObject facade;
        internal Animator originalAnimator;
        internal string lastClip = "";
        

        internal string GetCurrentClipOnOriginal()
        {
            var ClipInfos = originalAnimator.GetCurrentAnimatorClipInfo(0);
            if (ClipInfos.Length > 0)
            {
                return ClipInfos[0].clip?.name;
            }
            return null;
        }
        internal void PlayClip(string clipName) { 
            if(clipName != null && clipName != lastClip)
            {
                try
                {
                    if(animationSet.AnimationClips.TryGetValue(clipName,out var clip))
                    {
                        controller.Init(clip);
                    } else
                    {
                        Facade.Instance.Log($"clipName {clipName} not found in animation for custom animation {animationSet.Name} on {gameObject.GetName(true)}");
                    }
                }
                catch(Exception e) {
                    Facade.Instance.Log(clipName + "\n" + e.ToString());
                }
                lastClip = clipName;
            }
        }

        public void Update()
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
            gameObject.HideChildrenMatchingPath(animationSet.HideChildren);
            PlayClip(GetCurrentClipOnOriginal());
        }
    }
}
