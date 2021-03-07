using System;

namespace FileCatalog.App.Models
{
    public class FileEntryView
    {
        public long Id { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public string SizeText => $"{(Size / 1024.0):N1} KB";
        public DateTime Uploaded { get; set; }
        public string Location { get; set; }
    }
}
