using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IMovementDetailRepository
    {
        Task<MovementDetail> CreateAsync(MovementDetail detail);
    }
}
