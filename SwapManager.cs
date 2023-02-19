using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facade
{
    internal class SwapManager
    {
        internal static Dictionary<string, CustomAnimationSet> loadedCustomAnimationSet = new();
        internal static void loadCustomAnimationSet(string objectPath)
        {
            if (loadedCustomAnimationSet.TryGetValue(objectPath, out var tex))
            {
                return;
            }
            CustomAnimationSet buffer = new CustomAnimationSet(); ;
            string defaultDirectory = Path.Combine(CustomKnight.SkinManager.DATA_DIR, Facade.Instance.SWAP_FOLDER);
            var currentSkin = CustomKnight.SkinManager.GetCurrentSkin();
            string currentDirectory = Path.Combine(currentSkin.getSwapperPath(), Facade.Instance.SWAP_FOLDER);
            if (currentSkin.hasSwapper() && File.Exists(Path.Combine(currentDirectory, objectPath)))
            {
                buffer.DeserialiseFromFile(Path.Combine(currentDirectory, objectPath));
            }
            else if (File.Exists(Path.Combine(defaultDirectory, objectPath)))
            {
                buffer.DeserialiseFromFile(Path.Combine(defaultDirectory, objectPath));
            }
            else
            {
                return;
            }
            loadedCustomAnimationSet[objectPath] = buffer;
        }
    }
}
