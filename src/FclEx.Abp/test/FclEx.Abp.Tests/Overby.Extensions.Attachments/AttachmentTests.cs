namespace Overby.Extensions.Attachments;

public class AttachmentTests
{
    [Fact]
    public void FixedNullAttachmentResultImplicitOperator()
    {
        AttachmentResult<string>? r = null;
        string s = r;
        Assert.Null(s);
    }

    [Fact]
    public void CanRemoveAttachments()
    {
        var instance = new object();
        var key = Guid.NewGuid().ToString();
        instance.SetAttached("Ronnie", key);
        Assert.True(instance.GetAttached(key).Found);
        instance.RemoveAttached(key);
        Assert.False(instance.GetAttached(key).Found);
    }

    [Fact]
    public void CanClearAttachments()
    {
        var instance = new object();
        Assert.Empty(instance.GetAttachmentKeys());
        instance.Name("");
        Assert.NotEmpty(instance.GetAttachmentKeys());
        instance.ClearAttached();
        Assert.Empty(instance.GetAttachmentKeys());
    }

    [Fact]
    public void CanGetDefaultAttachmentValue()
    {
        var factoryInvoked = false;
        var instance = new object();
        var attachmentResult1 = instance.GetOrSetAttached(() =>
        {
            factoryInvoked = true;
            return new object();
        });

        Assert.True(factoryInvoked);
        Assert.False(attachmentResult1.Found);
        Assert.NotNull(attachmentResult1.Value);
        Assert.NotEqual(instance, attachmentResult1.Value);

        factoryInvoked = false;
        var attachmentResult2 = instance.GetOrSetAttached(() =>
        {
            factoryInvoked = true;
            return new object();
        });

        Assert.False(factoryInvoked);
        Assert.True(attachmentResult2.Found);
        Assert.NotNull(attachmentResult2.Value);
        Assert.NotEqual(instance, attachmentResult2.Value);
    }

    [Fact]
    public void Attachment_Found()
    {
        var instance = new object();
        instance.SetAttached(default(string));
        Assert.True(instance.GetAttached<string>().Found);
    }

    [Fact]
    public void Attachment_Not_Found()
    {
        var instance = new object();
        var attachment = instance.GetAttached<string>();
        Assert.False(attachment.Found);
    }

    [Fact]
    public void GetReferenceId_Returns_Unique_Guid_Per_Reference()
    {
        var instance1 = string.Intern("hello");
        var instance2 = new string(instance1.ToCharArray());

        // ensure refs not same
        Assert.False(ReferenceEquals(instance1, instance2));

        // ensure consistency
        Assert.Equal(instance1.GetReferenceId(), instance1.GetReferenceId());
        Assert.Equal(instance2.GetReferenceId(), instance2.GetReferenceId());

        // ensure unique ids
        Assert.NotEqual(instance1.GetReferenceId(), instance2.GetReferenceId());
    }

    [Fact]
    public void Boxing_Of_Value_Types_Prevents_Attaching_To_Them()
    {
        var n = 0;
        n.Name("Ronnie");
        string name = n.Name().Value;
        Assert.Null(name);
    }

    [Fact]
    public void CanCopyAttachments_WithPredicate()
    {
        const string expectedName = "Ronnie";
        var expectedId = new Random(0).Next();
        var o1 = new object();
        o1.Name(expectedName);
        o1.Id(expectedId);
        o1.Description("Some guy");

        var o2 = new object();

        // copy attachments from o1 -> o2
        // but not the description extension property
        o1.CopyAttachments(o2, k => k != o1.Description().AttachmentKey);

        Assert.Equal(expectedName, o2.Name().Value);
        Assert.Equal(expectedId, o2.Id().Value);
        Assert.Null(o2.Description().Value);
    }
}