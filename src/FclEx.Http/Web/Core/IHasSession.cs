using FclEx.Web.Models;

namespace FclEx.Web.Core
{
    public interface IHasSession
    {
        ISession Session { get; }
    }

    public interface IHasSession<out TSession>  where TSession : ISession
    {
        TSession Session { get; }
    }
}
