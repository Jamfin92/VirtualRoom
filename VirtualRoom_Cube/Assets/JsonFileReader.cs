using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonFileReader 
{

    public string LoadJsonAsResource(string path) //loads the .json file using its full name from the Resources folder.
    {
        string jsonFilePath = path.Replace(".json", ""); //TextAsset accepts json filetypes without the file designation (.json)
        TextAsset loadedJsonFile = Resources.Load<TextAsset>(jsonFilePath);
        return loadedJsonFile.text;
    }

}
