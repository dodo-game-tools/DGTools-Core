using UnityEngine;
using UnityEditor;

namespace DGTools.Editor {
    public class DGToolsWindow : EditorWindow
    {
        //VARIABLES
        GUISkin dgt_skin;

        //PROPERTIES
        protected string skinPath
        {
            get
            {
                return "Packages/com.dgtools.core/Editor/Resources/GUISkins/DGEditorSkin.guiskin";
            }
        }

        protected GUISkin skin
        {
            get
            {
                if (dgt_skin == null)
                {
                    dgt_skin = AssetDatabase.LoadAssetAtPath<GUISkin>(skinPath);

                    if (dgt_skin == null)
                    {
                        throw new System.Exception("No editor skin found at path " + skinPath);
                    }
                }

                return dgt_skin;
            }
        }
    }
}

