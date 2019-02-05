using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using NetMQ;

public class OSModule {

    enum MissionFileType
    {
        Metadata = 0,
        Mission,
        Map,
        Thumbnail
    }

    bool[] createdMissionFiles = new bool[4]{false, false, false, false};

    public static long bytesSent = 0;
    public static long updateDebFileSize = 0;
    long metadataFileSize = 0;
    long missionFileSize = 0;
    long mapFileSize = 0;
    long thumbnailFileSize = 0;

    ulong availableDiskSpace = 0;
    float realDiskSpace = 0.0f;
    int freeRam = 0;

    byte[] fileBytes = new byte[4096];
    FileStream fs;
    BinaryReader br;
    Dictionary<string, ulong> bytesPathDict = new Dictionary<string, ulong>();
    Dictionary<string, byte[]> chunkPathDict = new Dictionary<string, byte[]>();
    ClientUnity clientUnity;

    string dataPathUpdate;
    string fileNameUpdate;
    string updateDebFileServerPath, metadataFileServerPath, missionFileServerPath, mapFileServerPath, thumbnailFileServerPath;
    public static bool isUpdated = false;
    static QueueSync<string> metadataFiles = new QueueSync<string>(50);
    static QueueSync<string> guidToCreateButtons = new QueueSync<string>(50);

    public OSModule(string dataPath, string fileName)
    {
        clientUnity = UnityEngine.GameObject.Find("ClientUnity").GetComponent<ClientUnity>();
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_FILE_CREATED_NOTIFICATION, onFileCreated));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_FILE_CHUNK_WRITTEN_NOTIFICATION, onChunkWrite));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_FILE_CONTENT_NOTIFICATION, onContentReceived));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_FILE_ERROR_NOTIFICATION, onFileError));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_SYSTEM_INFO_NOTIFICATION, onSystemInfo));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_AVAILABLE_DISKSPACE_NOTIFICATION, onAvailableDiskspace));
        clientUnity.mhandlers.Add(new MsgHandler((byte)Modules.OS_NOTIFICATIONS_MODULE, (byte)OSNotificationType.OS_FILE_SIZE_NOTIFICATION, onFileSizeReceived));

        

        this.dataPathUpdate = dataPath;
        this.fileNameUpdate = fileName;
    }
    public static string getMetadataFile()
    {
        if (metadataFiles.GetSize() > 0)
            return metadataFiles.Dequeue();
        else
            return null;

    }
    public static string getGuidNameToCreateButton()
    {
        if (guidToCreateButtons.GetSize() > 0)
            return guidToCreateButtons.Dequeue();
        else
            return null;

    }
    public void onFileCreated(NetMQMessage m)
    {
        //UnityEngine.Debug.Log("File created");
        //UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(m[1].Buffer));
        string[] currentPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer).Split('.');
        //string[] currentPath = BitConverter.ToString(m[1].Buffer).Split('.');
        //UnityEngine.Debug.Log(currentPath[currentPath.Length - 1]);

        FileInfo fileInfo;
        //First we check if we haven't receive already that the files were created
        switch (currentPath[currentPath.Length-1])
        {
            case "metadata\0":
                if (!createdMissionFiles[(int)MissionFileType.Metadata])
                {
                    //If we get that the metadata was created, we check that it was created and get the info of the file. Reminder, the file is empty on the server
                    metadataFileServerPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
                    UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                    createdMissionFiles[(int)MissionFileType.Metadata] = true;

                    //UnityEngine.Debug.Log(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                    fileInfo = new System.IO.FileInfo(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                    //UnityEngine.Debug.Log(fileInfo.Length);
                    metadataFileSize = fileInfo.Length;

                }
                else
                    UnityEngine.Debug.Log("Metadata created flag already true");

                break;
            case "mission\0":
                if (!createdMissionFiles[(int)MissionFileType.Mission])
                {
                    //If we get that the mission was created, we check that it was created and get the info of the file. Reminder, the file is empty on the server

                    missionFileServerPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
                    //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                    createdMissionFiles[(int)MissionFileType.Mission] = true;

                    fileInfo = new System.IO.FileInfo(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.mission");
                    missionFileSize = fileInfo.Length;
                }

                break;
            case "map\0":
                if (!createdMissionFiles[(int)MissionFileType.Map]) {
                    //If we get that the map was created, we check that it was created and get the info of the file. Reminder, the file is empty on the server

                    mapFileServerPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
                    //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                    createdMissionFiles[(int)MissionFileType.Map] = true;
                    string metadata = File.ReadAllText(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                    MapMetadata mapMetadata = JsonUtility.FromJson<MapMetadata>(metadata);
                    if (mapMetadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
                    {
                        fileInfo = new System.IO.FileInfo(ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dpl.map");

                    }
                    else
                    {

                        fileInfo = new System.IO.FileInfo(ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dae.map");

                    }
                    mapFileSize = fileInfo.Length;
                }

                break;
            case "thumbnail\0":
                if (!createdMissionFiles[(int)MissionFileType.Thumbnail]){
                    //If we get that the thumbnail was created, we check that it was created and get the info of the file. Reminder, the file is empty on the server

                    thumbnailFileServerPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
                    //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                    createdMissionFiles[(int)MissionFileType.Thumbnail] = true;

                    fileInfo = new System.IO.FileInfo(ApplicationVariables.persistentDataPath + "Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail");
                    thumbnailFileSize = fileInfo.Length;
                }

                break;
            case "deb\0":
                //UnityEngine.Debug.Log("Created deb file on server");
                updateDebFileServerPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
                //Check if the update package was created on the server
                // Load file and first 4k bits
                fileInfo = new System.IO.FileInfo(dataPathUpdate + fileNameUpdate);
                updateDebFileSize = (long)fileInfo.Length;
                UnityEngine.Debug.Log(dataPathUpdate + fileNameUpdate);
                StartSendFile(dataPathUpdate + fileNameUpdate, updateDebFileServerPath);

                break;
            default:
                UnityEngine.Debug.Log("Unhandled file extension (Create File) :" + currentPath[currentPath.Length - 1]);
                break;
        }

        if (createdMissionFiles[(int)MissionFileType.Metadata] && createdMissionFiles[(int)MissionFileType.Mission] && (createdMissionFiles[(int)MissionFileType.Map] || PlanSelectionManager.uploadMapMetadata) && createdMissionFiles[(int)MissionFileType.Thumbnail])
        { 
            //If metadata, thumbnail, map and thumbnail was created, then we start sending the content of the files
            SendJSONStateMachine.pathsSend = true;
            createdMissionFiles[(int)MissionFileType.Metadata] = false;
            createdMissionFiles[(int)MissionFileType.Mission] = false;
            createdMissionFiles[(int)MissionFileType.Map] = false;
            createdMissionFiles[(int)MissionFileType.Thumbnail] = false;
            StartSendFile(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata", metadataFileServerPath);
        }
        
    }

    public void onChunkWrite(NetMQMessage m)
    {
        ulong size = BitConverter.ToUInt64(m[0].Buffer, 3);
        //bytesSent += (long)size;
        //UnityEngine.Debug.Log("Bytes sent: " + bytesSent);
        //We received that a chunk was written on the server 
        string[] currentPath = System.Text.Encoding.ASCII.GetString(m[1].Buffer).Split('.');
        //string[] currentPath = BitConverter.ToString(m[1].Buffer).Split('.');
        //UnityEngine.Debug.Log(currentPath[currentPath.Length - 1]);
        //We find which file was written first
        switch (currentPath[currentPath.Length - 1])
        {
            case "metadata\0":
                //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                //SendChunk returns true if the whole file was ulpoaded. If it is we continue onto the mission
                if (SendChunk(metadataFileServerPath, metadataFileSize, ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata"))
                    StartSendFile(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.mission", missionFileServerPath);
                break;
            case "mission\0":
                //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                //If the mission is uploaded, we check if is needed for the map to be uploaded. If it is not needed we upload the thumbnail, if it is, we upload the map
                if (SendChunk(missionFileServerPath, missionFileSize, ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.mission"))
                {
                    if (PlanSelectionManager.uploadMapMetadata)
                    {
                        StartSendFile(ApplicationVariables.persistentDataPath + "Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", thumbnailFileServerPath);
                    }
                    else
                    {
                        string metadata1 = File.ReadAllText(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                        MapMetadata mapMetadata1 = JsonUtility.FromJson<MapMetadata>(metadata1);
                        if (mapMetadata1.Map_type == (byte)MapMetadata.MapType.PointCloud)
                        {
                            StartSendFile(ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dpl.map", mapFileServerPath);

                        }
                        else
                        {

                            StartSendFile(ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dae.map", mapFileServerPath);

                        }


                    }
                }
                break;
            case "map\0":
                //Upload the thumbnail once the map finishes uploading
                //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                string metadata = File.ReadAllText(ApplicationVariables.persistentDataPath + "Missions/" + MissionManager.guid + ".json.metadata");
                MapMetadata mapMetadata = JsonUtility.FromJson<MapMetadata>(metadata);
                if (mapMetadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
                {
                    if (SendChunk(mapFileServerPath, mapFileSize, ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dpl.map"))
                        StartSendFile(ApplicationVariables.persistentDataPath + "Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", thumbnailFileServerPath);
                }
                else
                {

                    if (SendChunk(mapFileServerPath, mapFileSize, ApplicationVariables.persistentDataPath + "Maps/" + MissionManager.guid + ".dae.map"))
                        StartSendFile(ApplicationVariables.persistentDataPath + "Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail", thumbnailFileServerPath);
                }
                
                break;
            case "thumbnail\0":
                //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                //The chain ends here
                //SendJSONStateMachine.pathsSend = true;
                if (SendChunk(thumbnailFileServerPath, thumbnailFileSize, ApplicationVariables.persistentDataPath + "Thumbnails/" + MissionManager.guid + ".jpeg.thumbnail")) { 
                    UnityEngine.Debug.Log("All files sent");
                    SendJSONStateMachine.allFilesSent = true;
                    PlanSelectionManager.uploadMapMetadata = false;

                }
                break;
            case "deb\0":
                //UnityEngine.Debug.Log("File Created: " + BitConverter.ToString(m[1].Buffer));
                //While the server hasn't been updated and the file hasn't been uploaded, the update package gets uploaded. Once it is uploaded, we quit atreyu to update it.
                if (!isUpdated && SendChunk(updateDebFileServerPath, updateDebFileSize, dataPathUpdate + fileNameUpdate))
                {
                    clientUnity.client.SendCommand((byte)Modules.STD_COMMANDS_MODULE, (byte)CommandType.QUIT);
                    isUpdated = true;
                }

                break;
            default:
                UnityEngine.Debug.Log("Unhandled file extension (Chunk Write)");
                break;
        }        

    }
    public void onFileSizeReceived(NetMQMessage m)
    {
        //We received a file size
        //If we didn't ask for it, get out
        if (!PlanSelectionManager.askedForMaps )
        {
            return;
        }
        string currentFile = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        //Erase the \0
        string auxCurrentFile = currentFile.Substring(0, currentFile.Length - 1);

        ulong size = BitConverter.ToUInt64(m[0].Buffer, 3);
        //UnityEngine.Debug.Log("Got Size: " + size + " file: " + auxCurrentFile);
        //If the file is already in the arrays, we delete it
        if (bytesPathDict.ContainsKey(auxCurrentFile))
        {
            return;
        }
        //We add the path on the server along with how many bytes does the file weighs in a dictionary
        bytesPathDict.Add(auxCurrentFile, BitConverter.ToUInt64(m[0].Buffer, 3));
        //UnityEngine.Debug.Log("FileSizeReceived");
        //We send a request for the content of the file
        clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_CONTENT, 0, size, currentFile);
    }
    public void onContentReceived(NetMQMessage m)
    {

        ulong offset = BitConverter.ToUInt64(m[0].Buffer, 3);
        ulong size = BitConverter.ToUInt64(m[0].Buffer, 11);
        //UnityEngine.Debug.Log("Start writing file");

        string currentFile = System.Text.Encoding.ASCII.GetString(m[1].Buffer);
        currentFile = currentFile.Substring(0, currentFile.Length - 1);
        if (!bytesPathDict.ContainsKey(currentFile))
        {
            return;
        }
        //If the file has already been added ina a dictionary with the filename + chunks dowloaded, we continue downloading
        if (chunkPathDict.ContainsKey(currentFile))
        {

            byte[] array1 = chunkPathDict[currentFile];
            byte[] array2 = m[2].Buffer;
            if (array1.Length >= array2.Length)
            {
                //byte[] aux = new byte[array2.Length];
                //Array.Copy(array1, array1.Length - array2.Length, aux, 0, array2.Length);
                //if (array2 == aux)
                //{
                //    return;
                //}
                //if we already received the bytes, just in case there are two tablets receveing the same map, we exit
                if ((ulong)array1.Length >= offset)
                {
                    return;
                }

                //bool partAlreadyIn = true;
                //
                //for (int i = array2.Length - 1, j = array1.Length; i >= 0; i--, j--)
                //{
                //    if (array2[i] != array1[j])
                //    {
                //        partAlreadyIn = false;
                //        break;
                //
                //    }
                //
                //}
                //if (partAlreadyIn)
                //{
                //    return;
                //}
            }
            //We write the chunk received after the previous chunks
            byte[] newArray = new byte[(ulong)chunkPathDict[currentFile].Length + size];
            Array.Copy(array1, newArray, array1.Length);
            Array.Copy(array2, 0, newArray, array1.Length, array2.Length);
            chunkPathDict[currentFile] = newArray;
            //UnityEngine.Debug.Log("Writing file");
        }
        else
        {
            //Else, we add it to the dictionary
            //UnityEngine.Debug.Log("Began writing");
            chunkPathDict.Add(currentFile, new byte[size]);
            chunkPathDict[currentFile] = m[2].Buffer;
        }
        //If every byte has been written
        if ((ulong)chunkPathDict[currentFile].Length >= bytesPathDict[currentFile])
        {
            String objectString  = "";
           
            string[] extensionName = currentFile.Split('.');
            string[] path = extensionName[extensionName.Length - 3].Split('/');
            //Pointclouds aren't strings, so we don't convert yet for every type
            if (!extensionName[extensionName.Length - 1].Equals("map"))
                objectString = System.Text.Encoding.UTF8.GetString(chunkPathDict[currentFile]);
            //UnityEngine.Debug.Log(extensionName[extensionName.Length - 1]);
            if (extensionName[extensionName.Length-1].Equals("metadata"))
            {
                //UnityEngine.Debug.Log("It was metadata");

                metadataFiles.Enqueue(objectString);
            }
            else if (extensionName[extensionName.Length - 1].Equals("thumbnail"))
            {
                
                UnityEngine.Debug.Log(ApplicationVariables.persistentDataPath + "Thumbnails/" + path[path.Length - 1] + ".jpeg.thumbnail");

                System.IO.File.WriteAllBytes(ApplicationVariables.persistentDataPath + "Thumbnails/" + path[path.Length - 1] + ".jpeg.thumbnail", chunkPathDict[currentFile]);
                //UnityEngine.Debug.Log("Did it hang here?");

                clientUnity.client.sendTwoPartCommand((byte)Modules.PLAN_LIBRARIAN_MODULE, (byte)PlanLibrarianCommandType.PLAN_LIB_REQUEST_MISSION, path[path.Length - 1] + '\0');
               
            }
            else if (extensionName[extensionName.Length - 1].Equals("mission"))
            {
                //UnityEngine.Debug.Log("It was mission");

                System.IO.File.WriteAllText(ApplicationVariables.persistentDataPath + "Missions/" + path[path.Length - 1] + ".json.mission", objectString);
                guidToCreateButtons.Enqueue(path[path.Length - 1]);
               
                
            }
            else if (extensionName[extensionName.Length - 1].Equals("map"))
            {
                //UnityEngine.Debug.Log("It was map");
                if (GeneralSceneManager.sceneState == GeneralSceneManager.SceneState.Mapping)
                {
                    System.IO.File.WriteAllBytes(ApplicationVariables.persistentDataPath + "Maps/" + path[path.Length - 1] + ".dpl.map", chunkPathDict[currentFile]);
                }
                else
                {
                    string metadata = File.ReadAllText(ApplicationVariables.persistentDataPath + "Missions/" + path[path.Length - 1] + ".json.metadata");
                    MapMetadata mapMetadata = JsonUtility.FromJson<MapMetadata>(metadata);
                    if (mapMetadata.Map_type == (byte)MapMetadata.MapType.PointCloud)
                    {
                        System.IO.File.WriteAllBytes(ApplicationVariables.persistentDataPath + "Maps/" + path[path.Length - 1] + ".dpl.map", chunkPathDict[currentFile]);

                    }
                    else
                    {
                        objectString = System.Text.Encoding.UTF8.GetString(chunkPathDict[currentFile]);

                        System.IO.File.WriteAllText(ApplicationVariables.persistentDataPath + "Maps/" + path[path.Length - 1] + ".dae.map", objectString);

                    }
                }
                
                MapLoader.mapDownloaded = true;
                PlanSelectionManager.askedForMaps = false;

            }
            chunkPathDict.Remove(currentFile);
            bytesPathDict.Remove(currentFile);
            
        }
        else
        {
            //UnityEngine.Debug.Log("ContentReceived");
            clientUnity.client.sendTwoPartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_REQUEST_FILE_CONTENT, offset + size, size, System.Text.Encoding.ASCII.GetString(m[1].Buffer));
        }
    }
    public void onFileError(NetMQMessage m)
    {
        UnityEngine.Debug.Log("File error, not sudo permissions: " + System.Text.Encoding.ASCII.GetString(m[1].Buffer));
    }

    public void onSystemInfo(NetMQMessage m)
    {
        /*
         3 bytes cabecera +
         struct SystemInfo
        {
          long uptime;                                             Seconds since boot 
          float oneminuteload, fiveminuteload, fifteenminuteload;  1, 5, and 15 minute load averages 
          uint64_t totalram;                                       Total usable main memory size (bytes) 
          uint64_t freeram;                                        Available memory size (bytes) 
          uint64_t totalswap;                                      Total swap space size (bytes) 
          uint64_t freeswap;                                       swap space still available (bytes) 
          unsigned short procs;                                    Number of current processes 
        };
         
         */
    }

    public void onAvailableDiskspace(NetMQMessage m)
    {
        /*
         3 bytes cabecera +
         * uint64_t availableBytes; // free space
        uint64_t capacityBytes;  // free space + used space
         */

        availableDiskSpace = BitConverter.ToUInt64(m[0].Buffer, 3);
        realDiskSpace = availableDiskSpace / (1024 * 1024 * 1024.0f);
        //UnityEngine.Debug.Log("OCS available disk space: " + realDiskSpace + "GB");

    }

    void StartSendFile(string localPath, string serverPath)
    {
        if (fs != null)
        {
            fs.Close();
            br.Close();
        }
        //UnityEngine.Debug.Log("Start sending file: " + localPath);
        fs = File.OpenRead(localPath);
        br = new BinaryReader(fs);

        int readCount = br.Read(fileBytes, 0, 4096);
        UnityEngine.Debug.Log(localPath + ": " + readCount);
        bytesSent += readCount;
        if(readCount < 4096)
            Array.Resize<byte>(ref fileBytes, readCount);
        clientUnity.client.sendThreePartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_APPEND_DATA_FILE, serverPath, fileBytes);

    }
    /// <summary>
    /// SendChunk returns true if the whole file was ulpoaded. If it is we continue onto the mission
    /// </summary>
    /// <param name="serverPath"></param>
    /// <param name="fileSize"></param>
    /// <returns></returns>
    bool SendChunk(string serverPath, long fileSize, string localPath)
    {

        //UnityEngine.Debug.Log(bytesSent + "vs " + fileSize);
        if (bytesSent < fileSize)
        {
            int readCount = br.Read(fileBytes, 0, 4096); // Index es el indice de nuestro buffer, no del fichero a leer, ese puntero avanza de forma automática
            //UnityEngine.Debug.Log(serverPath + ": " + readCount);
            bytesSent += readCount;
            if (readCount < 4096) 
                Array.Resize<byte>(ref fileBytes, readCount);
                
            clientUnity.client.sendThreePartCommand((byte)Modules.OS_MODULE, (byte)OSCommandType.OS_APPEND_DATA_FILE, serverPath, fileBytes);
            return false;
        }
        else
        {
            // The package is in the server: sent a QUIT command to start installation
            br.Close();
            fs.Close();
            fs = null;
            br = null;
            bytesSent = 0;
            updateDebFileSize = 0;
            fileBytes = new byte[4096];
            return true;
        }
    }

}



