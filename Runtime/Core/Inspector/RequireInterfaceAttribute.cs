using System;
using UnityEngine;

namespace IIMLib.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequireInterfaceAttribute : PropertyAttribute
    {
        public Type RequiredType { get; }

        public RequireInterfaceAttribute(Type requiredType)
        {
            if (requiredType == null)
                throw new ArgumentNullException(nameof(requiredType));

            if (!requiredType.IsInterface)
                throw new ArgumentException("RequireInterfaceAttribute requires an interface type.", nameof(requiredType));

            RequiredType = requiredType;
        }
    }
}
