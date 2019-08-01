using UnityEngine;


public class FolderPathAttribute : PropertyAttribute {
    public bool hasClearButton = true;

    // ex : folderPathRestriction = "Resources" will force user to select a folder in "Assets/Resources/"
    public string folderPathRestriction = "";
}


