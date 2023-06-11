namespace FclEx.Web;

public interface IUserClientFactory<out TClient> where TClient : IUserClient
{
    IServiceProvider ServiceProvider { get; }
    TClient Create(IUserAccount account, IHttpService? httpService = null);
}