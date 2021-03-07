using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using FileCatalog.Respositories.DTO;
using FileCatalog.Respositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileCatalog.Respositories
{
    public class FileRepository : BaseRepository<FileEntry>, IFileRepository
    {
        public FileRepository(IConfiguration config, ILoggerFactory loggerFactory) :
            base(config, loggerFactory)
        {
        }

        public override async Task<IEnumerable<FileEntry>> GetAll()
        {
            /*
             * NOTE: We want our list of files sorted by Order.
             * In a real-world app this could bring a long list of entities and I'd probably
             * have it sorted in DB, however for sake of simplicity I opted to do it with Linq.
             */

            return (await base.GetAll()).OrderBy(f => f.Position).AsEnumerable();
        }

        public async Task<FileEntry> GetContent(int id)
        {
            using (Connection)
            {
                var fileHeader = await GetById(id);

                if (fileHeader != null)
                {
                    fileHeader.Content = (await Connection.GetAsync<FileContent>(id))?.Content;
                }

                return fileHeader;
            }
        }

        public async Task<FileEntry> Insert(IFormFile file)
        {
            var header = new FileEntry
            {
                Name = file.FileName,
                Size = file.Length,
                Type = file.ContentType,
                Uploaded = DateTime.Now,
            };

            // Read incomming stream
            using (var mem = new MemoryStream())
            {
                await file.CopyToAsync(mem);

                header.Content = mem.ToArray();
            }

            header.Id = await Insert(header);

            return header;
        }

        public override async Task<long> Insert(FileEntry header)
        {
            try
            {
                // Get next position
                header.Position = await GetNextPosition();

                long id;
                using (var transaction = new TransactionScope())
                {

                    id = await Connection.InsertAsync(header);
                    var content = new FileContent
                    {
                        FileId = id,
                        Content = header.Content
                    };
                    await Connection.InsertAsync(content);

                    transaction.Complete();
                }

                return id;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error saving file content for '{header.Name}'");
                throw;
            }
        }

        public async Task<bool> Remove(long id)
        {
            var header = await GetById(id);

            return header != null && await Remove(header);
        }


        /*
         * NOTE: Initially I  had overriden Remove() to have all adjacent items shifted to
         * avoid position gaps in the table. But thinking better about it, position can
         * be sparse as far as it is senquentially consistent.
         */

        //public override async Task<bool> Remove(FileHeader header)
        //{
        //    try
        //    {
        //        using (var transaction = new TransactionScope())
        //        {
        //            if (await base.Remove(header))
        //            {
        //                await ShiftPositions(header.Position, int.MaxValue);
        //            }

        //            transaction.Complete();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, $"Error removing file {header.Id}");
        //        throw;
        //    }
        //}

        public async Task<FileEntry> GetByPosition(int position)
        {
            return await Connection.QueryFirstOrDefaultAsync<FileEntry>($"SELECT * FROM {TableName} WHERE Position = {position}");
        }

        public async Task<FileEntry> Reorder(long id, int newPosition)
        {

            var header = await GetById(id);

            if (header != null)
            {
                Logger.LogDebug($"Moving file {id} from position {header.Position} to {newPosition}");

                var clash = await GetByPosition(newPosition);
                if (clash != null)
                {
                    await ShiftPositions(header.Position, newPosition);
                }

                header.Position = newPosition;
                await Update(header);
            }

            return header;
        }

        /*
         * NOTE: This method could be eliminated if the Position column in the
         * underlying table had a DEFAULT constraint configured with a DB function that
         * returned the next available position.
         * I've chosen this way bc it keeps it decopled from database and thus simpler.
         */

        private async Task<int> GetNextPosition()
        {
            var lastPos = await Connection.QueryFirstOrDefaultAsync<int>($"SELECT MAX({nameof(FileEntry.Position)}) AS Position FROM {nameof(FileEntry)}");
            return lastPos + 1;
        }

        private async Task ShiftPositions(int oldPos, int newPos)
        {
            if (oldPos == newPos)
            {
                return;
            }

            var step = 1;
            if (oldPos < newPos)
            {
                step = -1;
            }
            else
            {
                (oldPos, newPos) = (newPos, oldPos);
            }

            var posColumn = nameof(FileEntry.Position);

            var sql = $"UPDATE {TableName} SET {posColumn} = {posColumn} + ({step}) WHERE {posColumn} BETWEEN {oldPos - step} AND {newPos}";
            await Connection.ExecuteAsync(sql);
        }

    }
}
