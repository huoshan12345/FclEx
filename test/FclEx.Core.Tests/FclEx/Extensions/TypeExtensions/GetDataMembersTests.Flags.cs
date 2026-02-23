// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoProperty
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0649 // Field is never assigned

namespace FclEx.Extensions.TypeExtensions;

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
}
