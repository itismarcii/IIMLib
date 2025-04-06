using System;
using UnityEngine;

namespace IIMLib.Editor
{
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public Type RequiredType { get; private set; }
        
        /// <summary>
        /// Requiring implementation of the <see cref="T:RequireInterfaceAttribute"/> interface.
        /// </summary>
        /// <param name="type">Interface type.</param>
        public RequireInterfaceAttribute(Type type)
        {
            RequiredType = type;
        }
    }
}