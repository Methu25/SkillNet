using Microsoft.Data.SqlClient;
using SkillNet.Server.Models;

namespace SkillNet.Server.Services
{
    /// <summary>
    /// Singleton Pattern — Solves the problem of repeatedly querying the database
    /// for job categories that almost never change. Instead of hitting the DB on
    /// every API call, this class ensures a single global instance loads the
    /// categories once and serves them from an in-memory cache forever.
    ///
    /// Thread-safe using double-checked locking.
    /// NOT registered via Dependency Injection — callers use GetInstance() directly.
    /// </summary>
    public class JobCategoryService
    {
        private static JobCategoryService? _instance;
        private static readonly object _lock = new();

        private List<JobCategory>? _cachedCategories;

        // Private constructor — prevents external code from creating new instances
        private JobCategoryService() { }

        /// <summary>
        /// Returns the single global instance of JobCategoryService.
        /// Creates it on first call (thread-safe).
        /// </summary>
        public static JobCategoryService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new JobCategoryService();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Returns all job categories. Loads from DB only on the first call;
        /// all subsequent calls return the cached list.
        /// </summary>
        public async Task<List<JobCategory>> GetCategoriesAsync(string connectionString)
        {
            if (_cachedCategories == null)
            {
                _cachedCategories = await LoadFromDatabaseAsync(connectionString);
            }
            return _cachedCategories;
        }

        /// <summary>
        /// Clears the cache — call this if categories are ever updated via admin.
        /// </summary>
        public void InvalidateCache()
        {
            _cachedCategories = null;
        }

        private static async Task<List<JobCategory>> LoadFromDatabaseAsync(string connectionString)
        {
            var categories = new List<JobCategory>();
            const string query = "SELECT CategoryId, Name, Description FROM JobCategory ORDER BY Name";
            using var con = new SqlConnection(connectionString);
            await con.OpenAsync();
            using var cmd = new SqlCommand(query, con);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new JobCategory
                {
                    CategoryId = (int)reader["CategoryId"],
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString()
                });
            }
            return categories;
        }
    }
}
