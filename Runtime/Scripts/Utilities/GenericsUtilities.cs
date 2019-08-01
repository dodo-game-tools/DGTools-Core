using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace DGTools {
    public static class GenericsUtilities
    {
        /// <summary>Call a generic methods from a Type (Set reference to null for static methods)</summary>
        public static object Call(Type type, string methodName, object reference, object[] parameters = null) {

            MethodInfo method = type.GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
            object result = genericMethod.Invoke(null, parameters);

            return result;
        }
    }
}

