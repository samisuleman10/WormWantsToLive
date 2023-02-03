namespace Syncreality
{
    public interface IFileIOWrapper
    {
        public bool FileExists(string path);
        public bool DirectoryExists(string path);
        public string ReadAllText(string path);
        public void WriteAllText(string path, string text);
        public void CreateDirectory(string path);
        public string GetPathByPlatform();
        public void Delete(string path);
    }
}

