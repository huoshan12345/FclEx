using System;

namespace FclEx.Abp.Orm;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueSqlAttribute : Attribute
{
    public DefaultValueSqlAttribute(string defaultSql)
    {
        DefaultSql = defaultSql;
    }

    public string DefaultSql { get; }
}