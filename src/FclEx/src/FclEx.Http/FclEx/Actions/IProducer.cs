using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FclEx.Actions;

public interface IProducer<in TIn, TOut>
{
    Task<OperateResult<TOut>> ProduceAsync(TIn input);
}