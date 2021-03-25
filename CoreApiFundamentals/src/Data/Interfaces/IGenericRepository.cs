using CoreCodeCamp.Models;
using Microsoft.Extensions.Logging;
using SqlKata;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public interface IGenericRepository
    {
        // General 
        Task<bool> AddCamp(CampModel entity);
        Task<bool> AddTalk(TalkModel entity, string moniker);
        Task<bool> UpdateCamp(CampModel entity, string moniker);
        Task<bool> UpdateTalk(TalkModel entity, string moniker, int id);
        Task<bool> DeleteCamp(string moniker);
        Task<bool> DeleteTalk(string moniker, int id);
        Task<bool> SaveChangesAsync();
    }
    public class GenericRepository : IGenericRepository
    {
        private readonly CampContext _context;
        private readonly ILogger<CampRepository> _logger;
        private readonly IRepositoryBase _repository;

        public GenericRepository(CampContext context, ILogger<CampRepository> logger, IRepositoryBase repository)
        {
            _repository = repository;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> AddCamp(CampModel entity)
        {
            _logger.LogInformation($"Adding an object of type {entity.GetType()} to the context.");

            await _repository.ExecuteAsync(
                new Query("Location")
                .AsInsert(new
                {
                    VenueName = entity.Venue,
                    Address1 = entity.LocationAddress1,
                    Address2 = entity.LocationAddress2,
                    Address3 = entity.LocationAddress3,
                    CityTown = entity.LocationCityTown,
                    StateProvince = entity.LocationStateProvince,
                    PostalCode = entity.LocationPostalCode,
                    Country = entity.LocationCountry
                }));

            var locationId = await _repository.CreateAndReturnIdAsync(
                       new Query("Location")
                       .Select("LocationId")
                       .Where("Location.VenueName", entity.Venue));

            await _repository.ExecuteAsync(
                 new Query("Camps")
                 .AsInsert(new
                 {
                     Name = entity.Name,
                     Moniker = entity.Moniker,
                     EventDate = entity.EventDate,
                     Length = entity.Length,
                     LocationId = locationId
                 }));

            return true;
        }

        public async Task<bool> AddTalk(TalkModel entity, string moniker)
        {
            var speakerId = await GetOnlySpeakerId(entity);

            if (speakerId == 0)
            {
                speakerId = await _repository.CreateAndReturnIdAsync(
                new Query("Speakers")
                .AsInsert(new
                {
                    FirstName = entity.Speaker.FirstName,
                    LastName = entity.Speaker.LastName,
                    MiddleName = entity.Speaker.MiddleName,
                    Company = entity.Speaker.Company,
                    CompanyUrl = entity.Speaker.CompanyUrl,
                    BlogUrl = entity.Speaker.BlogUrl,
                    Twitter = entity.Speaker.Twitter,
                    GitHub = entity.Speaker.GitHub
                }));
            }

            var campId = await GetOnlyCampId(moniker);

            await _repository.ExecuteAsync(
                 new Query("Talks")
                 .AsInsert(new
                 {
                     CampId = campId,
                     Title = entity.Title,
                     Abstract = entity.Abstract,
                     Level = entity.Level,
                     SpeakerId = speakerId
                 }));

            return true;
        }

        public async Task<int> GetOnlySpeakerId(TalkModel entity)
        {
            return await _repository.GetAsync<int>(
                new Query("Speakers")
                .Select("SpeakerId")
                .Where("Speakers.FirstName", entity.Speaker.FirstName)
                .Where("Speakers.LastName", entity.Speaker.LastName));
        }

        public async Task<int> GetOnlyCampId(string moniker)
        {
            return await _repository.GetAsync<int>(
                new Query("Camps")
                .Select("CampId")
                .Where("Camps.Moniker", moniker));
        }

        public async Task<bool> UpdateCamp(CampModel entity, string moniker)
        {
            var campUpdate = await _repository.ExecuteAsync(
                new Query("Camps")
                .Where("Camps.Moniker", moniker)
                .AsUpdate(new
                {
                    Name = entity.Name,
                    Moniker = entity.Moniker,
                    EventDate = entity.EventDate,
                    Length = entity.Length
                })
                );

            var locationId = await _repository.GetAsync<int>(
                new Query("Camps")
                .Select("Camps.LocationId")
                .Where("Camps.moniker", moniker));

            var locationUpdate = await _repository.ExecuteAsync(
                new Query("Location")
                .Where("Location.LocationId", locationId)
                .AsUpdate(new
                {
                    VenueName = entity.Venue,
                    Address1 = entity.LocationAddress1,
                    Address2 = entity.LocationAddress2,
                    Address3 = entity.LocationAddress3,
                    CityTown = entity.LocationCityTown,
                    StateProvince = entity.LocationStateProvince,
                    PostalCode = entity.LocationPostalCode,
                    Country = entity.LocationCountry
                }));

            if (campUpdate && locationUpdate)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> UpdateTalk(TalkModel entity, string moniker, int id)
        {
            var talkUpdate = await _repository.ExecuteAsync(
                new Query("Talks")
                .Where("Talks.TalkId", id)
                .AsUpdate(new
                {
                    Title = entity.Title,
                    Abstract = entity.Abstract,
                    Level = entity.Level
                })
                );

            var speakerId = await GetOnlySpeakerId(entity);

            var speakerUpdate = await _repository.ExecuteAsync(
                new Query("Speakers")
                .Where("Speakers.SpeakerId", speakerId)
                .AsUpdate(new
                {
                    FirstName = entity.Speaker.FirstName,
                    LastName = entity.Speaker.LastName,
                    MiddleName = entity.Speaker.MiddleName,
                    Company = entity.Speaker.Company,
                    CompanyUrl = entity.Speaker.CompanyUrl,
                    BlogUrl = entity.Speaker.BlogUrl,
                    Twitter = entity.Speaker.Twitter,
                    GitHub = entity.Speaker.GitHub
                })
                );

            if (talkUpdate && speakerUpdate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteCamp(string moniker)
        {
            return await _repository.ExecuteAsync(
                new Query("Camps")
                .AsDelete()
                .Where("Camps.Moniker", moniker));

        }

        public async Task<bool> DeleteTalk(string moniker, int id)
        {
            return await _repository.ExecuteAsync(
                new Query("Talks")
                .Join("Camps", "Camps.CampId", "Talks.CampId")
                .AsDelete()
                .Where("Camps.Moniker", moniker)
                .Where("Talks.TalkId", id));
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
