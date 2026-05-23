// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable CS0169 // Field is never used
#pragma warning disable CA2211

using FclEx.TestModels;

namespace FclEx.Extensions.Reflection.TypeExtensions;

public class GetAllInstanceFieldsTests
{
    public class Struct
    {
        public int Property { get; set; }
        protected int PropertyProtected { get; set; }
        private int PropertyPrivate { get; set; }
        private int PropertyPrivateWithoutBackingField
        {
            get => PropertyPrivate;
            set => PropertyPrivate = value;
        }
        public static int PropertyStatic { get; set; }
        protected static int PropertyProtectedStatic { get; set; }
        private static int PropertyPrivateStatic { get; set; }
        private static int PropertyPrivateStaticWithoutBackingField
        {
            get => PropertyPrivateStatic; 
            set => PropertyPrivateStatic = value;
        }

        public int Field;
        protected int FieldProtected;
        private int FieldPrivate;
        public static int FieldStatic;
        protected static int FieldProtectedStatic;
        private static int FieldPrivateStatic;
    }

    public class BaseClass
    {
        public int Property { get; set; }
        protected int PropertyProtected { get; set; }
        private int PropertyPrivate { get; set; }
        public static int PropertyStatic { get; set; }
        protected static int PropertyProtectedStatic { get; set; }
        private static int PropertyPrivateStatic { get; set; }

        public int Field;
        protected int FieldProtected;
        private int FieldPrivate;
        public static int FieldStatic;
        protected static int FieldProtectedStatic;
        private static int FieldPrivateStatic;
    }

    public class InheritedClass : BaseClass
    {
        public int Property2 { get; set; }
        protected int PropertyProtected2 { get; set; }
        private int PropertyPrivate2 { get; set; }
        public static int PropertyStatic2 { get; set; }
        protected static int PropertyProtectedStatic2 { get; set; }
        private static int PropertyPrivateStatic2 { get; set; }

        public int Field2;
        protected int FieldProtected2;
        private int FieldPrivate2;
        public static int FieldStatic2;
        protected static int FieldProtectedStatic2;
        private static int FieldPrivateStatic2;
    }

    [Theory]
    [InlineData(typeof(object), 0)]
    [InlineData(typeof(string), 2)]
    [InlineData(typeof(Tuple<string, string>), 2)]
    [InlineData(typeof(Person), 6)]
    [InlineData(typeof(CommonClass), 3)]
    [InlineData(typeof(BaseClass), 6)]
    [InlineData(typeof(InheritedClass), 12)]
    [InlineData(typeof(CommonRecord), 3)]
    public void GetAllInstanceFields_Class_Test(Type type, int expectedCount)
    {
        var fields = type.GetAllInstanceFields();
        Assert.Equal(expectedCount, fields.Count);
    }

    [Theory]
    [InlineData(typeof(int), 1)]
    [InlineData(typeof(DateTime), 1)]
    [InlineData(typeof(ValueTuple<string, string>), 2)]
    [InlineData(typeof(Struct), 6)]
    [InlineData(typeof(CommonStruct), 3)]
    [InlineData(typeof(CommonRecordStruct), 3)]
    public void GetAllInstanceFields_Struct_Test(Type type, int expectedCount)
    {
        var fields = type.GetAllInstanceFields();
        Assert.Equal(expectedCount, fields.Count);
    }

    [Fact]
    public void GetAllInstanceFields_Interface_Test()
    {
        var fields = typeof(IReadOnlyList<int>).GetAllInstanceFields();
        Assert.Equal(0, fields.Count);
    }
}