using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FileCatalog.App.Models;
using FileCatalog.Respositories.DTO;
using FileCatalog.Respositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileCatalog.App.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        const int FILE_SIZE_LIMIT = 5 * 1024 * 1024; // as grandma always said: "5,000,000 is not 5MB"

        private readonly ILogger<FileController> _logger;
        private readonly IMapper _mapper;
        private readonly IFileRepository _repository;
        private readonly IEnumerable<string> _supporteFileTypes;

        public FileController(
            IConfiguration config,
            ILogger<FileController> logger,
            IMapper mapper,
            IFileRepository repository)
        {
            _logger = logger;
            _supporteFileTypes = config.GetSection("SupportedFileTypes").Get<string[]>();
            _mapper = mapper;
            _repository = repository;

            // NOTE: Usually I'd have an intermediate component between controller but again, let's KIS
        }


        /// <summary>
        /// Returns an entry in the file catalog.
        /// </summary>
        /// <param name="id">Identifier for the requested file.</param>
        /// <returns>Information about this specific file or HTTP 404 case it's not found.</returns>
        [HttpGet("{id:int}", Name = nameof(GetById))] // Required by CreatedAtRoute
        [ProducesResponseType(typeof(FileEntryView), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(MapToView(await _repository.GetByIdAsync(id)));
        }


        /// <summary>
        /// List all files ordered by their configured position.
        /// </summary>
        /// <returns>List of file entries in the catalog.</returns>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<FileEntryView>), StatusCodes.Status200OK)]
        public async Task<IEnumerable<FileEntryView>> GetAll()
        {
            return MapToView(await _repository.GetAllAsync());
        }


        /*
         * NOTE: As I imagined Swagger as the application "UI", I used the convenient
         * multi-part form file approach for download and upload, this way Swagger's OpenApi 3
         * spec will conveniently generate download and upload forms.
         */

        /// <summary>
        /// Downloads a file stored in the catalog.
        /// </summary>
        /// <remarks>
        /// Using Swagger UI, click at "Download File" link to get the result.
        /// If calling this endpoint form a client, use a stream reader to get file content.
        /// </remarks>
        /// <param name="id">File id</param>
        [HttpGet("download/{id:int}")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _repository.GetContentAsync(id);

            if (file == null)
            {
                return NotFound();
            }

            return File(file.Content, "application/octet-stream", file.Name);
        }


        /*
         * NOTE: Decision for RequestSizeLimit attribute to limit file size
         *          PROS - Simple to code
         *               - Automatically generates a 400 error
         *               - App doesn't need to receive the whole file  to check size
         *          CONS - It's not configurable via appsettings.
         */

        /// <summary>
        /// Uploads a file into the catalog.
        /// </summary>
        /// <remarks>
        /// The file is included in the end of the catalog.
        /// </remarks>
        /// <param name="file">Stream containing the content of the file to be uploaded.</param>
        /// <returns>A <see cref="FileEntryView"/> object containing the information about the stored file.</returns>
        [HttpPost("upload")]
        [RequestSizeLimit(FILE_SIZE_LIMIT)]
        [ProducesResponseType(typeof(FileEntryView), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if(!_supporteFileTypes.Contains(file.ContentType))
            {
                var errMsg = $"Invalid file format: '{file.ContentType}'. Current API version supports only '{string.Join("', '", _supporteFileTypes)}'";
                _logger.LogWarning(errMsg);
                return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                    new { statusCode = StatusCodes.Status415UnsupportedMediaType, title = errMsg });
            }

            var entry = MapToView(await _repository.InsertAsync(file));

            return CreatedAtRoute(nameof(this.GetById), new { id = entry.Id }, entry);
        }



        /// <summary>
        /// Remove a file from the catalog.
        /// </summary>
        /// <param name="id">File id.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task Remove(int id)
        {
            await _repository.RemoveAsync(id);
        }



        /// <summary>
        /// Reorders a file in the catalog, adjusting all adjacent file positions to reflect the change.
        /// </summary>
        /// <param name="id">ID fo the ile to be moved.</param>
        /// <param name="newPosition">Position inside catalago to meve this file.</param>
        /// <returns>Updated <see cref="FileEntryView"/> containing the new position.</returns>
        [HttpPut("{id:int}/reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reorder(long id, int newPosition)
        {
            var entry = await _repository.ReorderAsync(id, newPosition);
            if (entry == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(MapToView(entry));
            }
        }


        #region Helpers

        /*
         * NOTE: AutoMapper does most of boring work but as we need to call controller's
         * UrlHelper object to get location URL, I created these helper methods
         */

        private FileEntryView MapToView(FileEntry entry)
        {
            var view = _mapper.Map<FileEntryView>(entry);
            view.Location = Url.RouteUrl(nameof(GetById), new { id = view.Id }, HttpContext.Request.Scheme);
            return view;
        }

        private IEnumerable<FileEntryView> MapToView(IEnumerable<FileEntry> entries)
        {
            var views = _mapper.Map<IEnumerable<FileEntryView>>(entries);

            if (entries.Any())
            {
                // Let's cache this stuff
                var url = Url.RouteUrl(nameof(GetById), new { id = 0 }, HttpContext.Request.Scheme);
                url = url.Remove(url.Length - 1, 1) + "{0}";

                foreach (var view in views)
                {
                    view.Location = string.Format(url, view.Id);
                }
            }

            return views;
        }

        #endregion
    }
}
