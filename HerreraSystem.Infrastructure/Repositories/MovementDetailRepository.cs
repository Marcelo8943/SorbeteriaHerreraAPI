using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HerreraSystem.Application.Interfaces.Repositories;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class MovementDetailRepository : IMovementDetailRepository
    {
        private readonly HerreraSystemContext _context;

        public MovementDetailRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<MovementDetail> CreateAsync(MovementDetail detail)
        {
            _context.MovementDetails.Add(detail);
            await _context.SaveChangesAsync();
            return detail;   // EF ya populó el Id después del SaveChanges
        }
    }
}
