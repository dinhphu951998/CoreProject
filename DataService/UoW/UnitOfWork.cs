using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Data.Common;

namespace DataService.UoW
{
    public class UnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly DbConnection _connection;

        public UnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
            this._connection = _dbContext.Database.GetDbConnection();
        }

        public UnitOfWork(DbConnection connection)
        {
            this._connection = connection;
        }

        public IDbTransaction CreateTransaction()
        {
            return _connection.BeginTransaction();
        }


    }
}
