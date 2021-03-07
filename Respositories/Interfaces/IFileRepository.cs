using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileCatalog.Respositories.DTO;
using Microsoft.AspNetCore.Http;

namespace FileCatalog.Respositories.Interfaces
{
    public interface IFileRepository : IBaseRepository<FileEntry>
    {
        Task<FileEntry> GetContent(int id);

        Task<FileEntry> Insert(IFormFile file);

        Task<bool> Remove(long id);

        Task<FileEntry> GetByPosition(int position);

        Task<FileEntry> Reorder(long id, int newPosition);
    }
}