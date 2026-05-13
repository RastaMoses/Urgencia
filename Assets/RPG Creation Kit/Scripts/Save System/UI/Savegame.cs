using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System.IO;

namespace RPGCreationKit.SaveSystem
{
    public class Savegame : MonoBehaviour
    {
        public SaveFile saveFile;
        public FileInfo fileInfo;

        public void LoadSaveFile(FileInfo _fileInfo)
        {
            fileInfo = _fileInfo;
            saveFile = new SaveFile();
            StreamReader stream = _fileInfo.OpenText();
            saveFile = JsonUtility.FromJson<SaveFile>(stream.ReadToEnd());
            stream.Close();
        }
    }
}