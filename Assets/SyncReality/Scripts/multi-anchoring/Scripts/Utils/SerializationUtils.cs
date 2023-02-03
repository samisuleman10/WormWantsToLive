using System.IO;
using UnityEngine;

namespace Syncreality
{
    public class SerializationUtils : IFileIOWrapper
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string text)
        {
            File.WriteAllText(path, text);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public string GetPathByPlatform()
        {
            return Application.isEditor ? Application.dataPath : Application.persistentDataPath;
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }
    }
}
