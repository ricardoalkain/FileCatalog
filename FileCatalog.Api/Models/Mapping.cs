using AutoMapper;
using FileCatalog.Respositories.DTO;

namespace FileCatalog.App.Models
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<FileEntry, FileEntryView>();
        }
    }
}
