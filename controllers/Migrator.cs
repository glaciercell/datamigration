using System.Collections.Generic;
using System;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace datamigration
{

    [Route("api/Migrator")]
    public class Migrator : Controller
    {   
        private IDocumentClient _documentClient;
        private IConfiguration _configuration;

        public Migrator(IConfiguration configuration, IDocumentClient documentClient)
        {
            _configuration = configuration;
            _documentClient = documentClient;
        }

        [HttpGet("Migrate")]
        public void Migrate(IEnumerable<string> documentcollection)
        {
            var database = _configuration.GetSection("documentdb").GetSection("source").GetSection("database").Value;
            var db = _documentClient.CreateDatabaseQuery().ToList().FirstOrDefault(x => x.Id == database);
            var colls = _documentClient.CreateDocumentCollectionQuery(db.CollectionsLink).ToList();
            var migratecolls = colls.Where(z  => documentcollection.Contains(z.Id, StringComparer.InvariantCultureIgnoreCase));
            Parse(migratecolls);
        }

        private void Parse(IEnumerable<DocumentCollection> migratecolls)
        {
            try
            {
                foreach (var coll in migratecolls)
                {
                    var docs = _documentClient.CreateDocumentQuery(coll.DocumentsLink);

                    foreach (var doc in docs)
                    {
                        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "documents", coll.Id)))
                        {
                            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "documents", coll.Id));
                        }
                        var path = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "documents", coll.Id), doc.Id + ".json");
                        var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                        doc.SaveTo(fileStream, SerializationFormattingPolicy.Indented);
                    }

                }
            }
            catch (DocumentQueryException)
            {

            }

        }
    }
}