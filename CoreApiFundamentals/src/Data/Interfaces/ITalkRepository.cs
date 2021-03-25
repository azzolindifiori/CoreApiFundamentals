using Microsoft.Extensions.Logging;
using SqlKata;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public interface ITalkRepository
    {
        // Talks
        Task<Talk> GetTalkByMonikerAsync(string moniker, int talkId, bool includeSpeakers = false);
        Task<Talk[]> GetTalksByMonikerAsync(string moniker, bool includeSpeakers = false);
    }
    public class TalkRepository : ITalkRepository
    {
        private readonly ILogger<CampRepository> _logger;
        private readonly IRepositoryBase _repository;
        public TalkRepository(ILogger<CampRepository> logger, IRepositoryBase repository)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<Talk[]> GetTalksByMonikerAsync(string moniker, bool includeSpeakers = false)
        {

            _logger.LogInformation($"Getting all Talks for a Camp");

            var query = new Query("Talks")
                .Join("Camps", "Talks.CampId", "Camps.CampId")
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
                    , "Speakers.GitHub")
                .Where("Camps.Moniker", moniker);

            return (await _repository.GetListAsync(
                query,
                new Type[] { typeof(Talk), typeof(Speaker) },
                TalkMapper,
                splitOn: "SpeakerId")).ToArray();
        }

        public async Task<Talk> GetTalkByMonikerAsync(string moniker, int talkId, bool includeSpeakers = false)
        {
            _logger.LogInformation($"Getting all Talks for a Camp");

            var query = new Query("Talks")
                .Join("Camps", "Talks.CampId", "Camps.CampId")
                .Select(
                      "Talks.TalkId"
                    , "Talks.CampId"
                    , "Talks.Title"
                    , "Talks.Abstract"
                    , "Talks.Level"
                    , "Talks.SpeakerId")
                .Where("Camps.Moniker", moniker)
                .Where("Talks.TalkId", talkId);

            if (includeSpeakers)
            {
                query.Join("Speakers", "Speakers.SpeakerId", "Talks.SpeakerId")
                .Select(
                      "Speakers.SpeakerId"
                    , "Speakers.FirstName"
                    , "Speakers.LastName"
                    , "Speakers.MiddleName"
                    , "Speakers.Company"
                    , "Speakers.CompanyUrl"
                    , "Speakers.BlogUrl"
                    , "Speakers.Twitter"
                    , "Speakers.GitHub");

                return await _repository.GetAsync(
                query,
                new Type[] { typeof(Talk), typeof(Speaker) },
                TalkMapper,
                splitOn: "SpeakerId");
            }

            return await _repository.GetAsync<Talk>(query);
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
