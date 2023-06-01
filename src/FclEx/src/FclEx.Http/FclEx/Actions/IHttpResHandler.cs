using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Http;

namespace FclEx.Actions;

public interface IHttpResHandler<T>
{
    OperateResult<T> GetResult(HttpRes res);
}