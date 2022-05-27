using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FclEx.Http;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IHttpResHandler<T>
    {
        OperateResult<T> GetResult(HttpRes res);
    }
}
