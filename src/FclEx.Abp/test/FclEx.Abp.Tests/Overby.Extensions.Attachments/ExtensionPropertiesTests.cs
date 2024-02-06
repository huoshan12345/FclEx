
// these aliases just make the extension property signatures shorter

namespace Overby.Extensions.Attachments;

public class ExtensionPropertiesTests
{
    [Fact]
    public void Set_Via_Optional_Parameter_Get_Via_Value_Property()
    {
        var instance = new object();
        var now = DateTimeOffset.Now;

        // using the optional value parameter of the extension method
        // this is an alternative to `instance.Expiry().Value = now;`
        instance.Expiry(now);

        // using the `Value` property to get the value
        // this is an alternative to `DateTimeOffset expiry = instance.Expiry();`
        var expiry = instance.Expiry().Value;

        Assert.Equal(now, expiry);
    }

    [Fact]
    public void Set_Via_Value_Property_Get_Via_Implicit_Conversion()
    {
        var instance = new object();
        var now = DateTimeOffset.Now;

        //using the `Value` property to set the value
        // this is an alternative to `instance.Expiry(now);`
        instance.Expiry().Value = now;

        // using the implicit conversion operator to get the value
        // this is an alternative to `var expiry = instance.Expiry().Value;`
        DateTimeOffset expiry = instance.Expiry();

        Assert.Equal(now, expiry);
    }

    [Fact]
    public void Defaults_When_Not_Set_Via_Optional_Value_Parameter()
    {
        var instance = new object();
            
        int? id = instance.Id();
        string name = instance.Name();
        DateTimeOffset expiry = instance.Expiry();

        Assert.Equal(default(int?), id);
        Assert.Equal(default(string), name);
        Assert.Equal(default(DateTimeOffset), expiry);
    }

    [Fact]
    public void Property_Member_Name_Isolates_Multiple_Extension_Props_OfSame_Type()
    {
        var instance = new object();

        const string ExpectedName = "Ronnie";
        const string ExpectedDescription = "Some guy.";

        string name = instance.Name(ExpectedName);
        string description = instance.Description(ExpectedDescription);

        Assert.False(name == description, "these values should differ to prove this");
        Assert.Equal(ExpectedName, instance.Name().Value);
        Assert.Equal(ExpectedDescription, instance.Description().Value);
    }
}