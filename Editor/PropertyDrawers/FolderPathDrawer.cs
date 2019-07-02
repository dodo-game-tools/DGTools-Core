using UnityEngine;
using UnityEditor;
using System.IO;


[CustomPropertyDrawer(typeof(FolderPathAttribute))]
class ResourceFolderPathDrawer : PropertyDrawer
{
    //CONSTANTS
    const int BUTTONS_SIZE = 60;

    //PRIVATE VARIABLES
    bool modified = false;

    //EDITOR METHODS
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
                FolderPathAttribute attr = (FolderPathAttribute)attribute;

                string resourcesPath = Application.dataPath + "/";
                if (!string.IsNullOrEmpty(attr.folderPathRestriction)) resourcesPath += attr.folderPathRestriction + "/";
                string path = property.stringValue;
                var contentRect = EditorGUI.PrefixLabel(position, label);
                var textRect = contentRect;
                var ButtonRect = contentRect;

                textRect.width -= BUTTONS_SIZE * (attr.hasClearButton?2:1);
                ButtonRect.width = BUTTONS_SIZE;
                ButtonRect.x = textRect.xMax;

                path = EditorGUI.TextField(textRect, path);
                var select = GUI.Button(ButtonRect, "Browse");
                if (select)
                {
                    path = EditorUtility.OpenFolderPanel("Select Folder", resourcesPath + path, "");
                    modified = true;
                }

                if (attr.hasClearButton) {
                    ButtonRect.x += ButtonRect.width;
                    var clear = GUI.Button(ButtonRect, "Clear");
                    if (clear)
                    {
                        property.stringValue = null;
                    }
                }

                if (!string.IsNullOrEmpty(path))
                {
                    if (modified)
                    {
                        if (!path.Contains(resourcesPath))
                        {
                            Debug.Log("The Folder should be located in the "+ attr.folderPathRestriction + " folder");
                            modified = false;
                            return;
                        }
                        path = path.Replace(resourcesPath, "");
                        property.stringValue = path;
                        modified = false;
                    }

                }

                break;
            default:
                EditorGUI.LabelField(position, label.text, "Use ResourceFolderPathDrawer with string.");
                break;
        }

    }
}


