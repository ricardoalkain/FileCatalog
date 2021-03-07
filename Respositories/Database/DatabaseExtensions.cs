using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using FileCatalog.Respositories.DTO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace FileCatalog.Respositories.Database
{
    /// <summary>
    /// Quick and dirty solution to have my DB on startup (don't try this at home kids...)
    /// </summary>
    public static class DatabaseExtensions
    {
        public static void BuildDatabase(this IApplicationBuilder app, IConfiguration config, string schema = "dbo")
        {
            using (var connection = SqlClientFactory.Instance.CreateConnection())
            {
                connection.ConnectionString = config.GetConnectionString("SqlServer");
                connection.Open();

                var firstRun = (connection.QueryFirst<int>($"SELECT ISNULL(OBJECT_ID('{schema}.{nameof(FileEntry)}'), 0) AS TableId") == 0);

                connection.Execute($@"
                    IF OBJECT_ID('{schema}.{nameof(FileEntry)}') IS NULL
                    BEGIN
                        CREATE TABLE {schema}.{nameof(FileEntry)} (
                            [{nameof(FileEntry.Id)}]       BIGINT       NOT NULL IDENTITY(1,1) PRIMARY KEY,
                            [{nameof(FileEntry.Name)}]     VARCHAR(250) NOT NULL,
                            [{nameof(FileEntry.Type)}]     VARCHAR(20)  NOT NULL,
                            [{nameof(FileEntry.Position)}] INT          NOT NULL,
                            [{nameof(FileEntry.Size)}]     BIGINT       NOT NULL,
                            [{nameof(FileEntry.Uploaded)}] DATETIME     DEFAULT GETDATE()
                        );
                        CREATE NONCLUSTERED INDEX ix_CatalogFiles_Order ON {schema}.{nameof(FileEntry)}([{nameof(FileEntry.Position)}]);
                    END"
                );

                connection.Execute($@"
                    IF OBJECT_ID('{schema}.{nameof(FileContent)}') IS NULL
                    BEGIN
                        CREATE TABLE {schema}.{nameof(FileContent)} (
                            [{nameof(FileContent.FileId)}]  BIGINT PRIMARY KEY 
                                NOT NULL REFERENCES {nameof(FileEntry)}({nameof(FileEntry.Id)})
                                ON DELETE CASCADE,
                            [{nameof(FileContent.Content)}] VARBINARY(MAX)  NOT NULL
                        );
                    END"
                );

                /*
                 * NOTE: Populate some random values. Change dir value to some folder with PDF files
                 * to have a pre-populated DB to play around. Drop tables in database to repopulte them.
                 */

                var dir = config.GetSection("DBSeedFolder").Value;

                if (firstRun && Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir, "*.pdf")
                                         .Select(fileName => new FileInfo(fileName))
                                         .OrderByDescending(file => file.LastWriteTime)
                                         .ToArray();
                    if (files.Any())
                    {
                        var rnd = new Random();
                        for (int i = 0; i < files.Length; i++)
                        {
                            var header = new FileEntry
                            {
                                Name = files[i].Name,
                                Position = i + 1,
                                Size = files[i].Length,
                                Type = "application/pdf",
                                Uploaded = files[i].LastWriteTime,
                            };
                            if (connection.Get<FileEntry>(i + 1) == null)
                            {
                                var id = connection.Insert(header);
                                var content = new FileContent
                                {
                                    FileId = id,
                                    Content = File.ReadAllBytes(Path.Combine(dir, files[i].FullName))
                                };

                                connection.Insert(content);
                            }
                        }
                    }
                }
            }
        }
    }
}
