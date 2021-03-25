using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreCodeCamp.Data
{
    public class SqlKataCampRepository : ICampRepository
    {
        private readonly CampContext _context;
        private readonly ILogger<CampRepository> _logger;
        

        public SqlKataCampRepository(CampContext context, ILogger<CampRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add<T>(T entity) where T : class
        {
        }

        public void Delete<T>(T entity) where T : class
        {
        }

        public async Task<bool> SaveChangesAsync()
        {
            _logger.LogInformation($"Attempitng to save the changes in the context");

            // Only return success if at least one row was changed
            return (await _context.SaveChangesAsync()) > 0;
        }

        

        public async Task<Talk[]> GetTalksByMonikerAsync(string moniker, bool includeSpeakers = false)
        {
            _logger.LogInformation($"Getting all Talks for a Camp");

            IQueryable<Talk> query = _context.Talks;

            if (includeSpeakers)
            {
                query = query
                  .Include(t => t.Speaker);
            }

            // Add Query
            query = query
              .Where(t => t.Camp.Moniker == moniker)
              .OrderByDescending(t => t.Title);

            return await query.ToArrayAsync();
        }

        public async Task<Talk> GetTalkByMonikerAsync(string moniker, int talkId, bool includeSpeakers = false)
        {
            _logger.LogInformation($"Getting all Talks for a Camp");

            IQueryable<Talk> query = _context.Talks;

            if (includeSpeakers)
            {
                query = query
                  .Include(t => t.Speaker);
            }

            // Add Query
            query = query
              .Where(t => t.TalkId == talkId && t.Camp.Moniker == moniker);

            return await query.FirstOrDefaultAsync();
        }

        

        public Task<Camp[]> GetAllCampsAsync(bool includeTalks = false)
        {
            throw new NotImplementedException();
        }

        public Task<Camp> GetCampAsync(string moniker, bool includeTalks = false)
        {
            throw new NotImplementedException();
        }

        public Task<Camp[]> GetAllCampsByEventDate(DateTime dateTime, bool includeTalks = false)
        {
            throw new NotImplementedException();
        }
    }
}
