using FclEx.TestModels;
// ReSharper disable UnusedMember.Global
#pragma warning disable CA2211

namespace FclEx.Extensions.TypeExtensions;

public class GetAllInstanceFieldsTests
{
    public class BaseClass
    {
        public int Property { get; set; }
        protected int PropertyProtected { get; set; }
        public static int PropertyStatic { get; set; }
        protected static int PropertyProtectedStatic { get; set; }

        public int Field;
        protected int FieldProtected;
        public static int FieldStatic;
        protected static int FieldProtectedStatic;
    }

    public class InheritedClass : BaseClass
    {
        public int Property2 { get; set; }
        protected int PropertyProtected2 { get; set; }
        public static int PropertyStatic2 { get; set; }
        protected static int PropertyProtectedStatic2 { get; set; }

        public int Field2;
        protected int FieldProtected2;
        public static int FieldStatic2;
        protected static int FieldProtectedStatic2;
    }


    [Theory]
    [InlineData(typeof(Person), 6)]
    [InlineData(typeof(TestClass), 3)]
    [InlineData(typeof(BaseClass), 4)]
    [InlineData(typeof(InheritedClass), 8)]
    public void GetAllInstanceFields_Class_Test(Type type, int expectedCount)
    {
        var fields = type.GetAllInstanceFields();
        Assert.Equal(expectedCount, fields.Length);
    }
}