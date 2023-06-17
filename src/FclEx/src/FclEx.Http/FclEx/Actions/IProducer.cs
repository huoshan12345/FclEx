namespace FclEx.Actions;

public interface IProducer<in TIn, TOut>
{
    Task<OperateResult<TOut>> ProduceAsync(TIn input);
}