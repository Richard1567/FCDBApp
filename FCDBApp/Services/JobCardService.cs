using FCDBApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FCDBApp.Services
{
    public class JobCardService
    {
        private readonly InspectionContext _context;

        public JobCardService(InspectionContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobCard>> GetAllJobCardsAsync()
        {
            return await _context.JobCards.ToListAsync();
        }

        public async Task<JobCard> GetJobCardByIdAsync(Guid id)
        {
            return await _context.JobCards.FindAsync(id);
        }

        public async Task CreateJobCardAsync(JobCard jobCard)
        {
            _context.JobCards.Add(jobCard);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobCardAsync(JobCard jobCard)
        {
            _context.JobCards.Update(jobCard);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteJobCardAsync(Guid id)
        {
            var jobCard = await _context.JobCards.FindAsync(id);
            if (jobCard != null)
            {
                _context.JobCards.Remove(jobCard);
                await _context.SaveChangesAsync();
            }
        }
    }
}
