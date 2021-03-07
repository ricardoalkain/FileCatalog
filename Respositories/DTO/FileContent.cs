using Dapper.Contrib.Extensions;

namespace FileCatalog.Respositories.DTO
{
    /*
     * NOTE: To keep solution simple, I decided to store files in the database.
     * In a real-world app this would probably be my approach too, however I would
     * definitely for FILESTREAM if using SQL Server. This way files would be
     * eficiently stored in server's file system and still operating under transactions,
     * have their own file group(s) and more conveniently be a column in FileEntry
     * table without interfering with its performance.
     */

    [Table(nameof(FileContent))]
    public class FileContent
    {
        [ExplicitKey]
        public long FileId { get; set; }
        public byte[] Content { get; set; }
    }
}
