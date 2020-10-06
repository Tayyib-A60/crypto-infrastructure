using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.Persistence
{
    public interface IReadOnlyRepository<T>
        where T : IReadEntity
    {
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);

        Task<T> GetByIdAsync(string id);
        Task<T> GetByQueryString(string query);
        Task<IEnumerable<T>> GetAllByQueryString(string query);
    }
}