using System;
using Dapper.Contrib.Extensions;

namespace FileCatalog.Respositories.DTO
{
    [Table(nameof(FileEntry))]
    public class FileEntry
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public long Size { get; set; }

        public int Position { get; set; }

        public DateTime Uploaded { get; set; }

        [Computed]
        public byte[] Content { get; set; }
    }
}
