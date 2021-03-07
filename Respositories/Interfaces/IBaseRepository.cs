using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileCatalog.Respositories.Interfaces
{
    public interface IBaseRepository<T> : IDisposable where T : class
    {
        Task<long> Insert(T entity);
        Task<T> GetById(long id);
        Task<IEnumerable<T>> GetAll();
        Task<bool> Remove(T entity);
        Task<bool> Update(T entity);
    }
}