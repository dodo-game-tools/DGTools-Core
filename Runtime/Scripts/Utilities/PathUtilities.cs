using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGTools {
    public static class PathUtilities
    {
        public static string absolutePath => Application.isEditor ? Application.dataPath : Application.persistentDataPath;
    }
}

