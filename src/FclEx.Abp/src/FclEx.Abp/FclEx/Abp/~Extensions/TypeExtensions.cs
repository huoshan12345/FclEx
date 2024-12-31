using System;
using FclEx.Abp.Domain;

namespace FclEx.Abp;

public static class TypeExtensions
{
    public static bool IsEntity(this Type type)
    {
        return type is { IsGenericType: false, IsAbstract: false } && type.IsAssignableTo(typeof(IEntity));
    }
}