using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileCatalog.Respositories.Interfaces
{
    public interface IBaseRepository<T> : IDisposable where T : class
    {
        Task<long> InsertAsync(T entity);
        Task<T> GetByIdAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<bool> RemoveAsync(T entity);
        Task<bool> UpdateAsync(T entity);
    }
}