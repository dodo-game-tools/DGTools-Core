using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace DGTools {
    public static class GenericsUtilities
    {
        /// <summary>Calls a generic method of this kind : Class.Method<T>(T param) (Set reference to null for static methods)</summary>
        public static object CallMethod(Type ownerType, string methodName, Type paramType, object reference, params object[] parameters)
        {

            MethodInfo method = ownerType.GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(paramType);
            object result = genericMethod.Invoke(null, parameters);

            return result;
        }

        /// <summary>Calls a generic method of this kin : Class<T>.Method<T>(T param) (Set reference to null for static methods)</summary>
        public static object CallInnerMethod(Type ownerType, string methodName, object reference, params object[] parameters)
        {

            MethodInfo method = ownerType.GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(ownerType.GenericTypeArguments);
            object result = genericMethod.Invoke(null, parameters);

            return result;
        }
    }
}

