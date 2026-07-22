using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkillNet.Application.Interfaces;

namespace SkillNet.Infrastructure.Repositories
{
    public class AuthDbSession : IUnitOfWork
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;
        private SqlTransaction? _transaction;
        private bool _disposed;

        public AuthDbSession(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                    _connection.Open();
                }
                else if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public SqlTransaction? Transaction => _transaction;

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already active.");
            }

            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            else if (_connection.State == System.Data.ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    try
                    {
                        await _transaction.RollbackAsync();
                    }
                    catch
                    {
                        // Suppress errors during rollback on disposal
                    }
                    finally
                    {
                        await _transaction.DisposeAsync();
                        _transaction = null;
                    }
                }

                if (_connection != null)
                {
                    await _connection.DisposeAsync();
                    _connection = null;
                }

                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
