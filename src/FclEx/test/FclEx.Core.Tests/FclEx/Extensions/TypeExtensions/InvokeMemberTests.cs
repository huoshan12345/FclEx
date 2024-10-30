namespace FclEx.Extensions.TypeExtensions;

public class InvokeMemberTests
{
    public abstract class Tester<T>
    {
        public T? Property { get; set; }
        protected T? PropertyProtected { get; set; }
        public static T? PropertyStatic { get; set; }
        protected static T? PropertyProtectedStatic { get; set; }

        public T? Field;
        protected T? FieldProtected;
        public static T? FieldStatic;
        protected static T? FieldProtectedStatic;

        public void SetValues()
        {
            var random = new Random(0);
            Property = GenerateValue(random);
            PropertyProtected = GenerateValue(random);
            PropertyStatic = GenerateValue(random);
            PropertyProtectedStatic = GenerateValue(random);
            Field = GenerateValue(random);
            FieldProtected = GenerateValue(random);
            FieldStatic = GenerateValue(random);
            FieldProtectedStatic = GenerateValue(random);
        }

        public abstract T GenerateValue(Random random);

        public T Method(T arg) => arg;
        public static T MethodStatic(T arg) => arg;
        private T MethodProtected(T arg) => arg;
        private static T MethodProtectedStatic(T arg) => arg;
    }

    public class IntTester : Tester<int>
    {
        public override int GenerateValue(Random random) => random.Next(10000);
    }

    public class StringTester : Tester<string>
    {
        public override string GenerateValue(Random random) => random.NextString(5);
    }

    [Fact]
    public void GetMemberValue_Int_Test()
    {
        GetMemberValue_Test<IntTester, int>();
    }

    [Fact]
    public void GetMemberValue_String_Test()
    {
        GetMemberValue_Test<StringTester, string>();
    }

    private static void GetMemberValue_Test<T, TMember>() where T : Tester<TMember>, new()
    {
        var obj = new T();
        var type = typeof(T);
        const BindingFlags flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var members = new HashSet<DataMemberInfo>();
        while (type != null)
        {
            var ms = type.GetMembers(flag)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo())
                .Where(m => m.IsCompilerGenerated == false)
                .ToList();

            foreach (var member in ms)
            {
                if (member.IsStatic)
                {
                    var actual = type.GetDataMemberValue<TMember>(member.Name);
                    var expected = member.GetValue(null).CastTo<TMember>();
                    Assert.Equal(expected, actual);
                }
                else
                {
                    var actual = type.GetDataMemberValue<TMember>(member.Name, obj);
                    var expected = member.GetValue(obj).CastTo<TMember>();
                    Assert.Equal(expected, actual);
                }
                members.Add(member);
            }
            type = type.BaseType;
        }

        Assert.Equal(8, members.Count);
        Assert.Equal(4, members.Count(m => m.IsProperty));
        Assert.Equal(4, members.Count(m => m.IsField));
        Assert.Equal(4, members.Count(m => m.IsStatic));
        Assert.Equal(4, members.Count(m => !m.IsStatic));

    }
}