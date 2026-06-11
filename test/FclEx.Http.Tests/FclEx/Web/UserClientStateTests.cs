namespace FclEx.Web;

public class UserClientStateTests
{
    [Fact]
    public void SessionStatus_WhenValueChanges_RaisesChangedEvent()
    {
        var state = new UserClientState();
        var changes = new List<(UserClientSessionStatus OldValue, UserClientSessionStatus NewValue)>();
        state.SessionStatusChanged += (oldValue, newValue) => changes.Add((oldValue, newValue));

        state.SessionStatus = UserClientSessionStatus.LoggingIn;
        state.SessionStatus = UserClientSessionStatus.LoggingIn;
        state.SessionStatus = UserClientSessionStatus.Online;

        Assert.Equal(
        [
            (UserClientSessionStatus.Offline, UserClientSessionStatus.LoggingIn),
            (UserClientSessionStatus.LoggingIn, UserClientSessionStatus.Online),
        ], changes);
    }

    [Fact]
    public void AccountStatus_WhenValueChanges_RaisesChangedEvent()
    {
        var state = new UserClientState();
        var changes = new List<(UserAccountStatus OldValue, UserAccountStatus NewValue)>();
        state.AccountStatusChanged += (oldValue, newValue) => changes.Add((oldValue, newValue));

        state.AccountStatus = UserAccountStatus.Locked;
        state.AccountStatus = UserAccountStatus.Locked;
        state.AccountStatus = UserAccountStatus.Normal;

        Assert.Equal(
        [
            (UserAccountStatus.Normal, UserAccountStatus.Locked),
            (UserAccountStatus.Locked, UserAccountStatus.Normal),
        ], changes);
    }

    [Fact]
    public void SessionStatusExtensions_ReportCurrentSessionState()
    {
        var state = new UserClientState();

        state.LoggingIn();
        Assert.True(state.IsLoggingIn());
        Assert.False(state.IsAwaitingCaptcha());
        Assert.False(state.IsAwaitingChallenge());

        state.SessionStatus = UserClientSessionStatus.AwaitingCaptcha;
        Assert.True(state.IsAwaitingCaptcha());

        state.SessionStatus = UserClientSessionStatus.AwaitingChallenge;
        Assert.True(state.IsAwaitingChallenge());
    }

    [Fact]
    public void SessionStatusExtensions_UpdateSessionStatus()
    {
        var state = new UserClientState();

        state.Online();
        Assert.Equal(UserClientSessionStatus.Online, state.SessionStatus);

        state.Offline();
        Assert.Equal(UserClientSessionStatus.Offline, state.SessionStatus);
    }

    [Fact]
    public void IsAccountNormal_ReturnsTrueOnlyForNormalAccountStatus()
    {
        var state = new UserClientState();

        Assert.True(state.IsAccountNormal());

        state.AccountStatus = UserAccountStatus.InvalidCredentials;

        Assert.False(state.IsAccountNormal());
    }
}
