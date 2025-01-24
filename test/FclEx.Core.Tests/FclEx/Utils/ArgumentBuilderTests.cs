namespace FclEx.Utils;

public class ArgumentBuilderTests
{
    public interface IF;
    public class A : IF;
    public class B : A;

    public class Tester
    {
        public int CtorIndex { get; }
        public Tester(int a) { CtorIndex = 1; }
        public Tester(int a, long b) { CtorIndex = 2; }
        public Tester(int a, long b, double c) { CtorIndex = 3; }
        public Tester(int a, long b, double c, decimal d) { CtorIndex = 4; }
    }

    public class TesterOfSameParaType
    {
        public int CtorIndex { get; }
        public TesterOfSameParaType(int a) { CtorIndex = 1; }
        public TesterOfSameParaType(int a, int b) { CtorIndex = 2; }
        public TesterOfSameParaType(int a, long b, int c) { CtorIndex = 3; }
        public TesterOfSameParaType(int a, long b, int c, decimal d) { CtorIndex = 4; }
    }

    public class TesterOfDefaultPara
    {
        public int CtorIndex { get; }
        public TesterOfDefaultPara(int a) { CtorIndex = 1; }
        public TesterOfDefaultPara(int a, long b = 0) { CtorIndex = 2; }
        public TesterOfDefaultPara(long a, int b, long c = 0) { CtorIndex = 3; }
        public TesterOfDefaultPara(long a, long b, int c = 0, decimal d = 0) { CtorIndex = 4; }
    }

    public class TesterOfInherit
    {
        public int CtorIndex { get; }
        public TesterOfInherit(int a) { CtorIndex = 1; }
        public TesterOfInherit(int a, long b) { CtorIndex = 2; }
        public TesterOfInherit(long a, int b, object c) { CtorIndex = 3; }
        public TesterOfInherit(long a, long b, object c, object d) { CtorIndex = 4; }
        public TesterOfInherit(long a, long b, IF c, object d) { CtorIndex = 5; }
        public TesterOfInherit(long a, long b, A c, object d) { CtorIndex = 6; }
        public TesterOfInherit(long a, long b, object c, B d) { CtorIndex = 7; }
    }

    [Fact]
    public void CreateObject()
    {
        var obj = new ArgumentBuilder()
            .AddArgument(1)
            .CreateObject<Tester>();
        Assert.Equal(1, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .CreateObject<Tester>();
        Assert.Equal(2, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1.0)
            .CreateObject<Tester>();
        Assert.Equal(3, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1.0)
            .AddArgument((decimal)1)
            .CreateObject<Tester>();
        Assert.Equal(4, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(new object())
            .AddArgument(1)
            .AddArgument(new object())
            .AddArgument(1L)
            .AddArgument(new object())
            .AddArgument(1.0)
            .AddArgument(new object())
            .AddArgument((decimal)1)
            .AddArgument(new object())
            .CreateObject<Tester>();
        Assert.Equal(4, obj.CtorIndex);
    }

    [Fact]
    public void CreateObject_SameParaType()
    {
        var obj = new ArgumentBuilder()
            .AddArgument(1)
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(1, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1)
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(2, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1)
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(3, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1)
            .AddArgument((decimal)1)
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(4, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(new object())
            .AddArgument(1)
            .AddArgument(new object())
            .AddArgument(1L)
            .AddArgument(new object())
            .AddArgument(1.0)
            .AddArgument(new object())
            .AddArgument((decimal)1)
            .AddArgument(new object())
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(1, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(new object())
            .AddArgument(1)
            .AddArgument(new object())
            .AddArgument(1L)
            .AddArgument(new object())
            .AddArgument(1.0)
            .AddArgument(new object())
            .AddArgument((decimal)1)
            .AddArgument(new object())
            .AddArgument(1)
            .AddArgument(new object())
            .CreateObject<TesterOfSameParaType>();
        Assert.Equal(4, obj.CtorIndex);
    }

    [Fact]
    public void CreateObject_DefaultPara()
    {
        var obj = new ArgumentBuilder()
            .AddArgument(1)
            .CreateObject<TesterOfDefaultPara>();
        Assert.Equal(2, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .CreateObject<TesterOfDefaultPara>();
        Assert.Equal(3, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1.0)
            .CreateObject<TesterOfDefaultPara>();
        Assert.Equal(3, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument(1)
            .AddArgument((decimal)1)
            .CreateObject<TesterOfDefaultPara>();
        Assert.Equal(4, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(new object())
            .AddArgument(new object())
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument(new object())
            .AddArgument(1)
            .AddArgument(new object())
            .AddArgument((decimal)1)
            .AddArgument(new object())
            .AddArgument(1)
            .CreateObject<TesterOfDefaultPara>();
        Assert.Equal(4, obj.CtorIndex);
    }

    [Fact(Skip = "Need to be fixed")]
    public void CreateObject_TesterOfInherit()
    {
        var obj = new ArgumentBuilder()
            .AddArgument(1)
            .AddArgument(1L)
            .AddArgument(1.0)
            .CreateObject<TesterOfInherit>();
        Assert.Equal(3, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument(1)
            .AddArgument((decimal)1)
            .CreateObject<TesterOfInherit>();
        Assert.Equal(4, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument<IF>(new A())
            .AddArgument(new object())
            .CreateObject<TesterOfInherit>();
        Assert.Equal(5, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument(new A())
            .AddArgument(new object())
            .CreateObject<TesterOfInherit>();
        Assert.Equal(6, obj.CtorIndex);

        obj = new ArgumentBuilder()
            .AddArgument(1L)
            .AddArgument(1L)
            .AddArgument(new B())
            .AddArgument(new object())
            .CreateObject<TesterOfInherit>();
        Assert.Equal(6, obj.CtorIndex);

        var actual = new TesterOfInherit(1L, 1L, new B(), new object());
    }

}