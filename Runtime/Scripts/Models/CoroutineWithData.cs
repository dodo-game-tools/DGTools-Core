using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGTools {
    public class CoroutineWithData<Tdata>
    {
        public Coroutine coroutine { get; protected set; }
        public Tdata result;
        protected IEnumerator<Tdata> target;
        public CoroutineWithData(MonoBehaviour owner, IEnumerator<Tdata> target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        protected virtual IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }
}

