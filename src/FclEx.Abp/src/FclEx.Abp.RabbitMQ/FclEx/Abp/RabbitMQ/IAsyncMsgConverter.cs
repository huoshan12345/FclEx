using System.Threading.Tasks;

namespace FclEx.Abp.RabbitMQ
{
    public interface IAsyncMsgConverter<in TSource, TDestination>
    {
        Task<TDestination> Convert(TSource source);
    }
}
