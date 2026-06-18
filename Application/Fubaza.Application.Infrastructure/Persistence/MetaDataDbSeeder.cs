using Microsoft.Extensions.Logging;
using System.Reflection;

using Fubaza.Application.Core.Contracts.Serialization;
using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Entities;
using Fubaza.Application.Core.Extensions;


namespace Fubaza.Application.Infrastructure.Persistence
{
    public class MetaDataDbSeeder : IDatabaseSeeder
    {
        private readonly ILogger<MetaDataDbSeeder> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IJsonSerializer _jsonSerializer;

        public MetaDataDbSeeder(ILogger<MetaDataDbSeeder> logger, ApplicationDbContext db, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _db = db;
            _jsonSerializer = jsonSerializer;
        }
        public void Initialize()
        {
            //AddDesignation();
        }
        public void AddDesignation()
        {
            Task.Run(async () =>
            {
                try
                {
                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var data = await File.ReadAllTextAsync(path + @"/Persistence/SeedData/designation.json");
                    var result = _jsonSerializer.Deserialize<List<Designation>>(data);

                    if (result != null)
                    {
                        foreach (var designation in result)
                        {
                            if (string.IsNullOrWhiteSpace(designation.Title))
                                continue;

                            var existing = await _db.Designation.FindAsync(designation.Id);

                            if (existing != null)
                            {
                                // Update existing record
                                existing.Title = designation.Title;
                                existing.Department = designation.Department;
                                _db.Designation.Update(existing);
                            }
                            else
                            {
                                // Insert new record
                                await _db.Designation.AddAsync(designation);
                            }

                            await _db.SaveChangesAsync(); // Save after each operation
                        }
                    }

                    _logger.LogInformation("Seeded Designation");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception happened while seeding Designation data: {e.GetMessage()}");
                }
            }).GetAwaiter().GetResult();
        }

    }
}
