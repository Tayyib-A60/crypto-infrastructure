namespace WalletsCrypto.Domain.Core
{
    public interface IAggregate<TId>
    {
        TId Id { get; }
    }
}