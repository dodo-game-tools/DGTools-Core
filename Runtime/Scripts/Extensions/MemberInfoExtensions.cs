using System.Reflection;
using System;

namespace DGTools
{
	public static class MemberInfoExtensions
	{
        public static object GetValue(this MemberInfo memberInfo, object target)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(target);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(target);
                default:
                    throw new NotImplementedException();
            }
        }

        public static void SetValue(this MemberInfo memberInfo, object target, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(target, value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Type GetFieldType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
