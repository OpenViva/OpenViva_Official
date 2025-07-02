using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Viva.Util
{

    public static partial class Tools
    {

        public static bool EnsureFolder(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (System.Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        public static T LoadJson<T>(string filepath, object overwriteTarget = null) where T : class
        {
            if (File.Exists(filepath))
            {
                try
                {
                    if (overwriteTarget == null)
                    {
                        return JsonUtility.FromJson(File.ReadAllText(filepath), typeof(T)) as T;
                    }
                    else
                    {
                        JsonUtility.FromJsonOverwrite(File.ReadAllText(filepath), overwriteTarget);
                        return overwriteTarget as T;
                    }
                }
                catch (System.Exception e)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static bool SaveJson(object obj, bool prettyPrint, string path)
        {
            try
            {
                var json = JsonUtility.ToJson(obj, prettyPrint);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    byte[] data = Tools.UTF8ToByteArray(json);
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

    }
}