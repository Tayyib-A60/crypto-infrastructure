using System;
using System.Threading.Tasks;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.Persistence
{
    public interface IRepository<TAggregate, TAggregateId>
        where TAggregate: IAggregate<TAggregateId>
    {
        Task<TAggregate> GetByIdAsync(TAggregateId id);

        Task SaveAsync(TAggregate aggregate);
    }


    [Serializable]
    public class RepositoryException : Exception
    {
        public RepositoryException() { }
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception inner) : base(message, inner) { }
        protected RepositoryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}