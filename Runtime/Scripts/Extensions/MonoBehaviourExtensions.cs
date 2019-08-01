using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DGTools {
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Runs a <see cref="Coroutine"/> synchronously
        /// </summary>
        /// <param name="func">The <see cref="IEnumerator"/> method to run</param>
        public static void StartCoroutineSynchronous(this MonoBehaviour monoBehaviour, IEnumerator func)
        {
            while (func.MoveNext())
            {
                if (func.Current != null)
                {
                    IEnumerator num;
                    try
                    {
                        num = (IEnumerator)func.Current;
                    }
                    catch (InvalidCastException)
                    {
                        if (func.Current.GetType() == typeof(WaitForSeconds))
                            Debug.LogWarning("Skipped call to WaitForSeconds. Use WaitForSecondsRealtime instead.");
                        return;
                    }
                    StartCoroutineSynchronous(monoBehaviour, num);
                }
            }
        }
    }
}

