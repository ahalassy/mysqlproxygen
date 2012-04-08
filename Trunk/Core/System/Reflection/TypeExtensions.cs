using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection
{
    public static class TypeExtensions
    {
        public static object CreateInstance(this Type type)
        {
            return CreateInstance(type, new Type[] { }, new object[] { });
        }

        public static object CreateInstance(this Type type, Type[] argTypes, object[] args)
        {
            ConstructorInfo constructor = type.GetConstructor(argTypes);
            return constructor.Invoke(args);
        }

    }
}
