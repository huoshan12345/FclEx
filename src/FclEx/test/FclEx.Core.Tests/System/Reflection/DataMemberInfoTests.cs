// ReSharper disable UnusedAutoPropertyAccessor.Local

#pragma warning disable CS0414
#pragma warning disable IDE0051
namespace System.Reflection;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class DataMemberInfoTests(ITestOutputHelper output)
{
    public class Model
    {
        private static readonly int PrivateStaticField = 0;
        public static readonly int PublicStaticField = 0;
        private readonly int PrivateField = 0;
        public readonly int PublicField = 0;

        private static int PrivateStaticProperty { get; set; } = 0;
        public static int PublicStaticProperty { get; set; } = 0;
        private int PrivateProperty { get; set; } = 0;
        public int PublicProperty { get; set; } = 0;
        public int PublicPropertyWithPrivateSetter { get; private set; } = 0;
        public int PublicPropertyWithPrivateGetter { private get; set; } = 0;
        public int PublicPropertyWithoutSetter { get; } = 0;
    }

    [Fact]
    public void HashSet_Contains_Test()
    {
        var members = typeof(Model).GetDataMembers();
        Assert.Equal(11, members.Count);

        var set = members.ToHashSet();
        foreach (var member in typeof(Model).GetDataMembers())
        {
            Assert.Contains(member, set);
        }
    }

    [Fact]
    public void Equals_Test()
    {
        var count = 0;
        foreach (var member in typeof(Model).EnumerateDataMember())
        {
            Assert.Equal(member.ToDataMemberInfo(), member.ToDataMemberInfo());
            output.WriteLine(member.Name);

            count++;
        }
        Assert.Equal(18, count);
    }

    [Fact]
    public void Private_Static_Field_Test()
    {
        var field = typeof(Model).GetRequiredField("PrivateStaticField").ToDataMemberInfo();
        Assert.True(field.IsField);
        Assert.False(field.IsProperty);
        Assert.True(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.False(field.HasPublicSetter);
        Assert.False(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

    [Fact]
    public void Public_Static_Field_Test()
    {
        var field = typeof(Model).GetRequiredField("PublicStaticField").ToDataMemberInfo();
        Assert.True(field.IsField);
        Assert.False(field.IsProperty);
        Assert.True(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.True(field.HasPublicSetter);
        Assert.True(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

    [Fact]
    public void Private_Field_Test()
    {
        var field = typeof(Model).GetRequiredField("PrivateField").ToDataMemberInfo();
        Assert.True(field.IsField);
        Assert.False(field.IsProperty);
        Assert.False(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.False(field.HasPublicSetter);
        Assert.False(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

    [Fact]
    public void Public_Field_Test()
    {
        var field = typeof(Model).GetRequiredField("PublicField").ToDataMemberInfo();
        Assert.True(field.IsField);
        Assert.False(field.IsProperty);
        Assert.False(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.True(field.HasPublicSetter);
        Assert.True(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

    [Fact]
    public void Private_Static_Property_Test()
    {
        var field = typeof(Model).GetRequiredProperty("PrivateStaticProperty").ToDataMemberInfo();
        Assert.False(field.IsField);
        Assert.True(field.IsProperty);
        Assert.True(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.False(field.HasPublicSetter);
        Assert.False(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

    [Fact]
    public void Public_Static_Property_Test()
    {
        var field = typeof(Model).GetRequiredProperty("PublicStaticProperty").ToDataMemberInfo();
        Assert.False(field.IsField);
        Assert.True(field.IsProperty);
        Assert.True(field.IsStatic);

        Assert.True(field.CanRead);
        Assert.True(field.CanWrite);

        Assert.True(field.HasPublicSetter);
        Assert.True(field.HasPublicSetter);

        Assert.False(field.IsCompilerGenerated);
    }

}

file static class Extensions
{
    public static IEnumerable<MemberInfo> EnumerateDataMember(this Type type)
    {
        const BindingFlags flags = BindingFlags.Public
                                         | BindingFlags.NonPublic
                                         | BindingFlags.Instance
                                         | BindingFlags.Static;

        return type.GetFields(flags)
            .Cast<MemberInfo>()
            .Concat(type.GetProperties(flags));
    }
}