using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileCatalog.Respositories.DTO;
using Microsoft.AspNetCore.Http;

namespace FileCatalog.Respositories.Interfaces
{
    public interface IFileRepository : IBaseRepository<FileEntry>
    {
        Task<FileEntry> GetContentAsync(int id);

        Task<FileEntry> InsertAsync(IFormFile file);

        Task<bool> RemoveAsync(long id);

        Task<FileEntry> GetByPositionAsync(int position);

        Task<FileEntry> ReorderAsync(long id, int newPosition);
    }
}