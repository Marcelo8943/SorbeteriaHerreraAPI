using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using HerreraSystem.Application.Common;

namespace HerreraSystem.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HerreraSystemContext _context;
        private IDbContextTransaction? _transaction;
        private int _transactionDepth = 0;

        public UnitOfWork(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction is null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            _transactionDepth++;
        }

        public async Task CommitAsync()
        {
            if (_transactionDepth <= 0)
                return;

            _transactionDepth--;

            if (_transactionDepth == 0 && _transaction is not null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            _transactionDepth = 0;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            _transactionDepth = 0;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
