// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable ValueParameterNotUsed
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051

namespace FclEx.Extensions.Reflection.TypeExtensions;

public partial class GetDataMembersTests
{
    public class BaseModel
    {
        public int BasePublicField;
        protected int BaseProtectedField;
        private int BasePrivateField;

        public int BaseAuto { get; set; }
        public int BaseInit { get; init; }

        public static int BaseStaticField;
    }

    public class DerivedModel : BaseModel
    {
        public int PublicField;
        internal int InternalField;
        private readonly int ReadonlyField;

        public int Auto { get; set; }
        public int InitOnly { get; init; }

        public static int StaticField;

        public int ManualProp
        {
            get => _manual;
            set => _manual = value;
        }
        private int _manual;
    }

    public class InitOnlyTestClass
    {
        public int NormalProp { get; set; }

        public int InitProp { get; init; }

        public int ReadOnlyProp => 42;

        public readonly int ReadonlyField = 10;

        public int NormalField;
    }

    public class IndexerTestClass
    {
        public int NormalProperty { get; set; }

        public int this[int i]
        {
            get => i;
            set { }
        }

        public string this[string key]
        {
            get => key;
            set { }
        }

        private int PrivateProperty { get; set; }
    }

    public class DerivedWithIndexer : IndexerTestClass;

    [Fact]
    public void DeclaredOnly_Should_Exclude_BaseMembers()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.Public |
                DataMemberFlags.Field |
                DataMemberFlags.CanRead
            )
            .ToList();

        Assert.DoesNotContain(members, m => m.Name.Contains("Base"));
    }

    [Fact]
    public void InheritedOnly_Should_Exclude_DeclaredMembers()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Inherited |
                DataMemberFlags.Instance |
                DataMemberFlags.Public |
                DataMemberFlags.Field |
                DataMemberFlags.CanRead
            )
            .ToList();

        Assert.All(members, m => Assert.NotEqual(typeof(DerivedModel), m.DeclaringType));
    }

    [Fact]
    public void StaticFilter_Should_Return_Only_Static()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Static |
                DataMemberFlags.Public |
                DataMemberFlags.Field |
                DataMemberFlags.CanRead
            );

        Assert.All(members,
            m => Assert.True(m.IsStatic));
    }

    [Fact]
    public void Field_Should_Exclude_AutoPropertyBackingField_ByDefault()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.NonPublic |
                DataMemberFlags.Field |
                DataMemberFlags.CanRead
            )
            .ToList();

        Assert.Contains(members,
            m => m.Name == "_manual");

        Assert.DoesNotContain(members,
            m => m.Name.Contains("<Auto>"));
    }

    [Fact]
    public void AutoPropertyBackingField_Should_Include_CompilerGeneratedFields()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.NonPublic |
                DataMemberFlags.Field |
                DataMemberFlags.AutoPropertyBackingField |
                DataMemberFlags.CanRead
            )
            .ToList();

        Assert.Contains(members, m => m.Name == "_manual");

        Assert.Contains(members, m => m.Name.Contains("<Auto>"));
    }

    [Fact]
    public void UnsafeWrite_Should_Include_Readonly_And_Init()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.NonPublic |
                DataMemberFlags.Field |
                DataMemberFlags.CanWrite |
                DataMemberFlags.UnsafeWrite
            )
            .ToList();

        Assert.Contains(members, m => m.Name.Contains("ReadonlyField"));
    }

    [Fact]
    public void Missing_Declared_And_Inherited_Should_Return_Empty()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Instance |
                DataMemberFlags.Public |
                DataMemberFlags.Field |
                DataMemberFlags.CanRead
            );

        Assert.Empty(members);
    }

    [Fact]
    public void Missing_ReadWrite_Filter_Should_Return_Empty()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.Public |
                DataMemberFlags.Field
            );

        Assert.Empty(members);
    }

    [Fact]
    public void Property_Should_Not_Return_BackingField_Even_With_AutoPropertyBackingField()
    {
        var members = typeof(DerivedModel)
            .GetDataMembers(
                DataMemberFlags.Declared |
                DataMemberFlags.Instance |
                DataMemberFlags.NonPublic |
                DataMemberFlags.Property |
                DataMemberFlags.AutoPropertyBackingField |
                DataMemberFlags.CanRead
            );

        Assert.DoesNotContain(members, m => m.Name.Contains("k__BackingField"));
    }

    [Fact]
    public void Property_CanWrite_Should_Exclude_Init_Without_UnsafeWrite()
    {
        var members = typeof(InitOnlyTestClass)
            .GetDataMembers(
                DataMemberFlags.Property |
                DataMemberFlags.Public |
                DataMemberFlags.Instance |
                DataMemberFlags.CanWrite |
                DataMemberFlags.Declared)
            .ToList();

        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.NormalProp));
        Assert.DoesNotContain(members, m => m.Name == nameof(InitOnlyTestClass.InitProp));
    }

    [Fact]
    public void Property_CanWrite_Should_Include_Init_With_UnsafeWrite()
    {
        var members = typeof(InitOnlyTestClass)
            .GetDataMembers(
                DataMemberFlags.Property |
                DataMemberFlags.Public |
                DataMemberFlags.Instance |
                DataMemberFlags.CanWrite |
                DataMemberFlags.Declared |
                DataMemberFlags.UnsafeWrite)
            .ToList();

        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.NormalProp));
        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.InitProp));
    }

    [Fact]
    public void Field_Should_Exclude_Readonly_Without_UnsafeWrite()
    {
        var members = typeof(InitOnlyTestClass)
            .GetDataMembers(
                DataMemberFlags.Field |
                DataMemberFlags.Public |
                DataMemberFlags.Instance |
                DataMemberFlags.CanWrite |
                DataMemberFlags.Declared)
            .ToList();

        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.NormalField));
        Assert.DoesNotContain(members, m => m.Name == nameof(InitOnlyTestClass.ReadonlyField));
    }

    [Fact]
    public void Field_Should_Include_Readonly_With_UnsafeWrite()
    {
        var members = typeof(InitOnlyTestClass)
            .GetDataMembers(
                DataMemberFlags.Field |
                DataMemberFlags.Public |
                DataMemberFlags.Instance |
                DataMemberFlags.CanWrite |
                DataMemberFlags.Declared |
                DataMemberFlags.UnsafeWrite)
            .ToList();

        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.NormalField));
        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.ReadonlyField));
    }

    [Fact]
    public void Property_CanRead_Should_Always_Include_Init()
    {
        var members = typeof(InitOnlyTestClass)
            .GetDataMembers(
                DataMemberFlags.Property |
                DataMemberFlags.Public |
                DataMemberFlags.Instance |
                DataMemberFlags.CanRead |
                DataMemberFlags.Declared)
            .ToList();

        Assert.Contains(members, m => m.Name == nameof(InitOnlyTestClass.InitProp));
    }

    [Fact]
    public void IsInitOnly_Should_Work()
    {
        var normal = typeof(InitOnlyTestClass)
            .GetProperty(nameof(InitOnlyTestClass.NormalProp))!;

        var init = typeof(InitOnlyTestClass)
            .GetProperty(nameof(InitOnlyTestClass.InitProp))!;

        Assert.False(normal.IsInitOnly());
        Assert.True(init.IsInitOnly());
    }

    [Fact]
    public void GetDataMembers_WithoutIndexerFlag_Should_Exclude_Indexers()
    {
        var members = typeof(IndexerTestClass)
            .GetDataMembers(DataMemberFlags.Property
                            | DataMemberFlags.Public
                            | DataMemberFlags.Instance
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Declared)
            .ToArray();

        Assert.NotEmpty(members);

        Assert.DoesNotContain(members, m =>
            m.MemberInfo is PropertyInfo p &&
            p.GetIndexParameters().Length > 0);
    }

    [Fact]
    public void GetDataMembers_WithIndexerFlag_Should_Include_Indexers()
    {
        var members = typeof(IndexerTestClass)
            .GetDataMembers(DataMemberFlags.Property
                            | DataMemberFlags.Public
                            | DataMemberFlags.Instance
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Indexer
                            | DataMemberFlags.Declared);

        var indexers = members
            .Select(m => m.MemberInfo)
            .OfType<PropertyInfo>()
            .Where(p => p.GetIndexParameters().Length > 0)
            .ToList();

        Assert.NotEmpty(indexers);
        Assert.Equal(2, indexers.Count);
    }

    [Fact]
    public void GetDataMembers_WithIndexerAndNonPublic_Should_RespectVisibility()
    {
        var members = typeof(IndexerTestClass)
            .GetDataMembers(DataMemberFlags.Property
                            | DataMemberFlags.NonPublic
                            | DataMemberFlags.Instance
                            | DataMemberFlags.Indexer
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Declared)
            .ToArray();

        Assert.NotEmpty(members);

        Assert.DoesNotContain(members, m =>
            m.MemberInfo is PropertyInfo p &&
            p.GetIndexParameters().Length > 0);
    }

    [Fact]
    public void GetDataMembers_WithoutPropertyFlag_Should_NotReturn_Indexers()
    {
        var members = typeof(IndexerTestClass)
            .GetDataMembers(DataMemberFlags.Indexer
                            | DataMemberFlags.Public
                            | DataMemberFlags.Instance
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Declared)
            .ToArray();

        Assert.Empty(members);

        Assert.DoesNotContain(members, m => m.MemberInfo is PropertyInfo);
    }

    [Fact]
    public void GetDataMembers_WithIndexerFlag_Should_Return_Both_Normal_And_Indexer()
    {
        var members = typeof(IndexerTestClass)
            .GetDataMembers(DataMemberFlags.Property
                            | DataMemberFlags.Public
                            | DataMemberFlags.Instance
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Indexer
                            | DataMemberFlags.Declared);

        var props = members
            .Select(m => m.MemberInfo)
            .OfType<PropertyInfo>()
            .ToList();

        Assert.Contains(props, p => p.Name == nameof(IndexerTestClass.NormalProperty));
        Assert.Contains(props, p => p.GetIndexParameters().Length > 0);
    }

    [Fact]
    public void GetDataMembers_InheritedIndexer_Should_Work()
    {
        var members = typeof(DerivedWithIndexer)
            .GetDataMembers(DataMemberFlags.Property
                            | DataMemberFlags.Public
                            | DataMemberFlags.Instance
                            | DataMemberFlags.CanRead
                            | DataMemberFlags.Indexer
                            | DataMemberFlags.Inherited);

        Assert.Contains(members, m =>
            m.MemberInfo is PropertyInfo p &&
            p.GetIndexParameters().Length > 0);
    }
}
