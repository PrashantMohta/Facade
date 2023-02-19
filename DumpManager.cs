using CustomKnight.Canvas;
using System.IO;
using System.Linq;
using static Satchel.GameObjectUtils;
using static Satchel.IoUtils;

namespace Facade
{
    public class DumpManager{
        internal static void saveCustomAnimationJson(CustomAnimationSet cAnimSet, string sceneName, string objectName)
        {
            string DUMP_DIR = Path.Combine(Facade.Instance.DATA_DIR, "Dump");
            string scenePath = Path.Combine(DUMP_DIR, sceneName);

            EnsureDirectory(DUMP_DIR);
            EnsureDirectory(scenePath);

            string outpath = Path.Combine(scenePath, objectName.Replace('/', Path.DirectorySeparatorChar) + ".json");
            try
            {
                EnsureDirectory(Path.GetDirectoryName(outpath));
            }
            catch (IOException e)
            {
                Facade.Instance.Log(e.ToString());
            }
            cAnimSet.SerialiseToFile(outpath);
        }
    }
}