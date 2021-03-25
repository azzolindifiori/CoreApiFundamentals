using Microsoft.Extensions.Logging;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public interface ICampRepository
    {
        Task<Camp[]> GetAllCampsAsync(bool includeTalks = false);
        Task<Camp> GetCampAsync(string moniker, bool includeTalks = false);
        Task<Camp[]> GetAllCampsByEventDate(DateTime dateTime, bool includeTalks = false);
    }
        public class CampRepository : ICampRepository
        {
            private readonly ILogger<CampRepository> _logger;
            private readonly IRepositoryBase _repository;

        public CampRepository(ILogger<CampRepository> logger, IRepositoryBase repository)
        {
            _repository = repository;
            _logger = logger;;
        }

        public async Task<Camp[]> GetAllCampsByEventDate(DateTime dateTime, bool includeTalks = false)
            {
                _logger.LogInformation($"Getting all Camps");

                var query = new Query("Camps")
                    .Join("Location", "Location.LocationId", "Camps.LocationId")
                    .Select("Camps.CampId", "Camps.Name", "Camps.Moniker", "Camps.LocationId", "Camps.EventDate", "Camps.Length",
                    "Location.LocationId", "Location.VenueName", "Location.Address1", "Location.Address2", "Location.Address3",
                    "Location.CityTown", "Location.StateProvince", "Location.PostalCode", "Location.Country")
                    .Where("Camps.EventDate", dateTime);

                var camps = await _repository.GetListAsync(
                    query,
                    new Type[] { typeof(Camp), typeof(Location) },
                    CampMapper,
                    splitOn: "LocationId");

                if (includeTalks)
                {
                    foreach (var camp in camps)
                    {
                        camp.Talks = (ICollection<Talk>)(await GetTalksByCampIdAsync(camp.CampId));
                    };
                }

                return camps.ToArray();
            }

            public async Task<Camp[]> GetAllCampsAsync(bool includeTalks = false)
            {
                _logger.LogInformation($"Getting all Camps");

                var query = new Query("Camps")
                    .Join("Location", "Location.LocationId", "Camps.LocationId")
                    .Select("Camps.CampId", "Camps.Name", "Camps.Moniker", "Camps.LocationId", "Camps.EventDate", "Camps.Length",
                    "Location.LocationId", "Location.VenueName", "Location.Address1", "Location.Address2", "Location.Address3",
                    "Location.CityTown", "Location.StateProvince", "Location.PostalCode", "Location.Country");

                var camps = await _repository.GetListAsync(
                    query,
                    new Type[] { typeof(Camp), typeof(Location) },
                    CampMapper,
                    splitOn: "LocationId");

                if (includeTalks)
                {
                    foreach (var camp in camps)
                    {
                        camp.Talks = (ICollection<Talk>)(await GetTalksByCampIdAsync(camp.CampId));
                    };
                }
                return camps.ToArray();
            }

            public async Task<Camp> GetCampAsync(string moniker, bool includeTalks = false)
            {
                _logger.LogInformation($"Getting a Camp for {moniker}");

                var query = new Query("Camps")
                    .Join("Location", "Location.LocationId", "Camps.LocationId")
                    .Select("Camps.CampId", "Camps.Name", "Camps.Moniker", "Camps.LocationId", "Camps.EventDate", "Camps.Length",
                    "Location.LocationId", "Location.VenueName", "Location.Address1", "Location.Address2", "Location.Address3",
                    "Location.CityTown", "Location.StateProvince", "Location.PostalCode", "Location.Country")
                    .Where("Camps.Moniker", moniker);

                var camps = await _repository.GetAsync(
                    query,
                    new Type[] { typeof(Camp), typeof(Location) },
                    CampMapper,
                    splitOn: "LocationId");

                if (includeTalks)
                {
                    camps.Talks = (ICollection<Talk>)(await GetTalksByCampIdAsync(camps.CampId));
                }

                return camps;
            }

            private const string tableName = "Talks";
            private readonly Query _queryBase = new Query(tableName);
            public async Task<IEnumerable<Talk>> GetTalksByCampIdAsync(int id)
            {
                return await _repository.GetListAsync<Talk>(
                    _queryBase
                    .Clone()
                    .Join("Speakers", "Speakers.SpeakerId", "Talks.SpeakerId")
                    .Select(
                          "Talks.TalkId"
                        , "Talks.CampId"
                        , "Talks.Title"
                        , "Talks.Abstract"
                        , "Talks.Level"
                        , "Talks.SpeakerId"
                        , "Speakers.SpeakerId"
                        , "Speakers.FirstName"
                        , "Speakers.LastName"
                        , "Speakers.MiddleName"
                        , "Speakers.Company"
                        , "Speakers.CompanyUrl"
                        , "Speakers.BlogUrl"
                        , "Speakers.Twitter"
                        , "Speakers.GitHub"
                        )
                    .Where("Talks.CampId", id),
                    new Type[] { typeof(Talk), typeof(Speaker) },
                    TalkMapper,
                    splitOn: "SpeakerId");
            }

            private Camp CampMapper(object[] obj)
            {
                var c = obj[0] as Camp;
                var l = obj[1] as Location;

                c.Location = l;

                return c;
            }

            private Talk TalkMapper(object[] obj)
            {
                var t = obj[0] as Talk;
                var s = obj[1] as Speaker;

                t.Speaker = s;

                return t;
            }
        }
    }
