using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public interface ISpeakerRepository
    {
        Task<Speaker[]> GetSpeakersByMonikerAsync(string moniker);
        Task<Speaker> GetSpeakerAsync(int speakerId);
        Task<Speaker[]> GetAllSpeakersAsync();
    }
        public class SpeakerRepository : ISpeakerRepository
        {
            private readonly CampContext _context;
            private readonly ILogger<CampRepository> _logger;

        public SpeakerRepository(CampContext context, ILogger<CampRepository> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Speaker[]> GetSpeakersByMonikerAsync(string moniker)
            {
                _logger.LogInformation($"Getting all Speakers for a Camp");

                IQueryable<Speaker> query = _context.Talks
                  .Where(t => t.Camp.Moniker == moniker)
                  .Select(t => t.Speaker)
                  .Where(s => s != null)
                  .OrderBy(s => s.LastName)
                  .Distinct();

                return await query.ToArrayAsync();
            }

            public async Task<Speaker[]> GetAllSpeakersAsync()
            {
                _logger.LogInformation($"Getting Speaker");

                var query = _context.Speakers
                  .OrderBy(t => t.LastName);

                return await query.ToArrayAsync();
            }


            public async Task<Speaker> GetSpeakerAsync(int speakerId)
            {
                _logger.LogInformation($"Getting Speaker");

                var query = _context.Speakers
                  .Where(t => t.SpeakerId == speakerId);

                return await query.FirstOrDefaultAsync();
            }
        }
  }
