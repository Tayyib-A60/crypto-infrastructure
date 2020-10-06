using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.Persistence
{
    public interface IRepository<T> : IReadOnlyRepository<T>
        where T : IReadEntity
    {
        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);
    }
}