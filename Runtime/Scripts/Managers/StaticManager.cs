using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGTools {
    public class StaticManager<TManager> : MonoBehaviour where TManager : MonoBehaviour
    {
        //STATIC VARIABLES
        public static TManager active { get; private set; }

        //VARIABLES
        public bool dontDestroyOnLoad;

        //RUNTIME METHODS
        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                TManager obj = FindObjectOfType<TManager>();

                if (obj != null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    active = GetComponent<TManager>();
                }

                DontDestroyOnLoad(active.gameObject);
            }
            else
            {
                active = GetComponent<TManager>();
            }
        }
    }
}

