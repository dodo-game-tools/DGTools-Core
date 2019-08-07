using UnityEngine;
using System;

namespace DGTools
{
    public class TypeConstraintAttribute : PropertyAttribute
    {
        public TypeConstraintAttribute(Type type)
        {
            this.type = type;
        }

        public Type type { get; private set; }
    }
}

