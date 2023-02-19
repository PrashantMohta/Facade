namespace Facade
{
    public class CustomAnimationSet
    {
        public string Name = "";
        public bool Enabled = false;
        public float OffsetX = 0f;
        public float OffsetY = 0f;
        public float OffsetZ = 0f;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        public bool HasDefaultAnimation = false;
        public string DefaultAnimation = "";
        public List<string> HideChildren = new();
        public Dictionary<string, Satchel.Animation> AnimationClips = new();


        public void Load(CustomAnimationSet source)
        {
            Name = source.Name;
            Enabled = source.Enabled;
            OffsetX = source.OffsetX;
            OffsetY = source.OffsetY;
            OffsetZ = source.OffsetZ;
            ScaleX = source.ScaleX;
            ScaleY = source.ScaleY;
            HasDefaultAnimation = source.HasDefaultAnimation;
            DefaultAnimation = source.DefaultAnimation;
            HideChildren = source.HideChildren;
            AnimationClips = source.AnimationClips;
        }
    }
}