using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;  //File class

public static class FileManager {

    public static string persistentMappingDataPath = Application.persistentDataPath + "/PersistentData/Mapping/";
    public static string persistentPlanningDataPath = Application.persistentDataPath + "/PersistentData/Planning/";

    public static string debFilePath = Application.persistentDataPath + "/PersistentData/Update/";
    public static string debFileName = "airt-project";

    public static void MoveAllMappingFiles()
    {
        //This was sued to move an old debug pointcloud
        /*
        // Obtiene la lista de todos los archivos del directorio /Resources/Mapping/
        TextAsset[] ta = Resources.LoadAll<TextAsset>("Mapping");

        // Crea el directorio para los datos persistentes de mapping si no existe
        if (!Directory.Exists(persistentMappingDataPath))
            Directory.CreateDirectory(persistentMappingDataPath);

        // Mueve todos los archivos del directorio mapping de resources al persistente.
        foreach (TextAsset t in ta)
        {
            //UnityEngine.Debug.Log("Text Asset: " + t.name);
            if (!File.Exists(persistentMappingDataPath + t.name)) { 
            //    File.Move(fileInfo.ToString(), persistentMappingDataPath + fileInfo.Name);

                using (FileStream fs = new FileStream(persistentMappingDataPath + t.name, FileMode.OpenOrCreate))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(t.bytes, 0, t.bytes.Length);
                    }
                    fs.Dispose();
                    fs.Close();
                    UnityEngine.Debug.Log("Saving file " + persistentMappingDataPath);
                }
            }
        }*/

    }

    public static void MoveDebFile()
    {
        //When called, this moves the .bytes file of an update to the persistendata folder, in order to be sent to the drone
        TextAsset ta = Resources.Load<TextAsset>("Update/" + debFileName);
        UnityEngine.Debug.Log(ta.name);
        FileStream fs = null;
        try
        {
            fs = new FileStream(debFilePath + ta.name + ".deb", FileMode.OpenOrCreate);
            fs.Close();
        }
        catch (System.Exception)
        {

            return;
        }
        // Crea el directorio para los datos de actualización persistentes si no existe
        if (!Directory.Exists(debFilePath))
            Directory.CreateDirectory(debFilePath);

        using (fs = new FileStream(debFilePath + ta.name + ".deb", FileMode.OpenOrCreate))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(ta.bytes, 0, ta.bytes.Length);
                bw.Close();
            }
            
            fs.Dispose();
            fs.Close();
            
            UnityEngine.Debug.Log("Saving file " + debFilePath + ta.name);
        }
    }

    public static int GetNumFiles(string path)
    {
        //gets the number of files in a path
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] files = info.GetFiles("*");
        return files.Length;
    }

    public static byte[] ReadFile(string path)
    {

        byte[] fileBytes = File.ReadAllBytes(path);

        if (fileBytes != null)
        {
            return fileBytes;
        }
        else
        {
            return null;
        }

    }
}
