using System.IO;
using UnityEngine;
using Syncreality;

public class SerializationHandler
{
	public IFileIOWrapper FileIOWrapper;
	public SerializationHandler(IFileIOWrapper fileIOWrapper)
	{
		FileIOWrapper = fileIOWrapper;
	}

    public virtual string Serialize(string path, string fileName, string data, string extension/*"/AnchoringSerialization, ".json""*/)
	{
        var rootPath = FileIOWrapper.GetPathByPlatform();
        Debug.Log("From SerializationHandler, dataPath = " + rootPath);
        var directoryPath = rootPath + path;
        Debug.Log("From SerializationHandler, directoryPath = " + directoryPath);

        if (!FileIOWrapper.DirectoryExists(directoryPath))
        {
            Debug.Log("Directory doesnt Exist, " + directoryPath);
            FileIOWrapper.CreateDirectory(directoryPath);
        }

        var filePath = Path.Combine(rootPath + path, fileName + extension);

        Debug.Log("Serialize file to: " + filePath);
        Debug.Log("Serialize data = " + data);

        FileIOWrapper.WriteAllText(filePath, data);
        return filePath;
    }

    public virtual string SerializeWithoutRootPath(string path, string fileName, string data, string extension/*"/AnchoringSerialization, ".json""*/)
    {
        if (!FileIOWrapper.DirectoryExists(path))
        {
            Debug.Log("Directory doesnt Exist, " + path);
            FileIOWrapper.CreateDirectory(path);
        }

        var filePath = Path.Combine(path, fileName + extension);

        Debug.Log("Serialize file to: " + filePath);
        Debug.Log("Serialize data = " + data);

        FileIOWrapper.WriteAllText(filePath, data);
        return filePath;
    }

    public virtual string Deserialize(string path)
    {
        //var rootPath = FileIOWrapper.GetPathByPlatform();
        //var filePath = rootPath + path;
        
        if (!FileIOWrapper.FileExists(path))
        {
            Debug.Log("File " + path + " doesn't exist.");
            return null;
        }
        string data = FileIOWrapper.ReadAllText(path);
        Debug.Log("Deserialize data = " + data);

        Debug.Log("From Deserialize, filePath = " + path);

        return data;
    }
}
