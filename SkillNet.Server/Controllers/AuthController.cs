using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SkillNet.Server.Models;
using SkillNet.Server.Utilities;

namespace SkillNet.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection")!;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            string hashedPassword = PasswordHasher.HashPassword(request.Password);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Find Role ID based on name string
                string roleQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
                int roleId = 0;
                using (SqlCommand cmd = new SqlCommand(roleQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", request.RoleName);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return BadRequest("Invalid Role Specified.");
                    roleId = (int)result;
                }

                // Insert new user record
                string insertQuery = "INSERT INTO Users (Email, PasswordHash, RoleID) VALUES (@Email, @PasswordHash, @RoleID)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@RoleID", roleId);

                    try { cmd.ExecuteNonQuery(); }
                    catch (SqlException) { return BadRequest("User already exists."); }
                }
            }
            return Ok(new { message = "Registration successful!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"SELECT u.UserID, u.Email, u.PasswordHash, r.RoleName 
                                 FROM Users u JOIN Roles r ON u.RoleID = r.RoleID 
                                 WHERE u.Email = @Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return Unauthorized("Invalid email or password.");

                        string dbHash = reader["PasswordHash"].ToString()!;
                        if (!PasswordHasher.VerifyPassword(request.Password, dbHash))
                            return Unauthorized("Invalid email or password.");

                        string roleName = reader["RoleName"].ToString()!;
                        string token = GenerateJwtToken(request.Email, roleName);

                        return Ok(new { Token = token, Role = roleName, Message = "Login successful" });
                    }
                }
            }
        }

        private string GenerateJwtToken(string email, string role)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}