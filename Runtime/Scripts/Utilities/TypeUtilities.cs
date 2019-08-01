using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TypeUtilities : MonoBehaviour
{

    /// <summary>
    /// Converts a string into a type. Search type in assemblies if type not found
    /// </summary>
    /// <param name="typeString">The name of the desired type</param>
    /// <returns>The desired type (null if not found)</returns>
    public static Type GetTypeFromString(string typeString) {
        Type type = Type.GetType(typeString);

        if (type != null) return type;
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            type = assembly.GetType(typeString);
            if (type != null)
                return type;
        }

        return null;
    }
}
