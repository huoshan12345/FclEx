namespace FclEx.Extensions.TypeExtensions;

public partial class GetDataMembersTests
{
    public interface I0
    {
        int Member0 { get; set; }
    }

    public interface I1 : I0
    {
        int Member1 { get; set; }
    }

    public interface I2 : I0
    {
        new int Member0 { get; set; }
        int Member1 { get; set; }
    }

    public interface I3 : I0
    {
        new int Member0 { get; set; }
        int Member1 { get; set; }
    }

    [Fact]
    public void Interface_Test()
    {
        var members = typeof(I0).GetDataMembers();
        Assert.Equal(1, members.Count);
    }

    [Fact]
    public void Interface_Inherit_Test()
    {
        var members = typeof(I1).GetDataMembers();
        Assert.Equal(2, members.Count);
    }

    [Fact]
    public void Interface_Inherit_WithNew_Test()
    {
        {
            var members = typeof(I2).GetDataMembers();
            Assert.Equal(3, members.Count);
        }
        {
            var members = typeof(I3).GetDataMembers();
            Assert.Equal(3, members.Count);
        }
    }
}