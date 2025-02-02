using System.Security.Cryptography;
using LogisticsAPI.Models;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using LogisticsAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;

namespace LogisticsAPI.Controllers;

[Route("api/login")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        using var connection = new DatabaseConnection().GetConnection();
        connection.Open();

        var command = new SqliteCommand("SELECT * FROM Dispatcher WHERE Username = @Username", connection);
        command.Parameters.AddWithValue("@Username", request.Username);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return Unauthorized("Invalid username or password.");
        }

        var passwordHash = reader["PasswordHash"].ToString();
        if (!VerifyPassword(request.Password, passwordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok(GenerateAccessToken(reader.GetBoolean(2)));
    }

    private bool VerifyPassword(string inputPassword, string storedPasswordHash)
    {
        var inputPasswordBytes = Encoding.UTF8.GetBytes(inputPassword);
        var inputPasswordHash = SHA256.HashData(inputPasswordBytes);
        var inputPasswordHashString = BitConverter.ToString(inputPasswordHash).Replace("-", "");
        return inputPasswordHashString.ToLower() == storedPasswordHash.ToLower();
    }

    private string GenerateAccessToken(bool canEditDispatchers)
    {

        var key = new SymmetricSecurityKey("secret-key-51067322564376754607543076524076"u8.ToArray());
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new Claim[]
            {
                new("CanEditDispatchers", canEditDispatchers.ToString())
            },
            expires: DateTime.Now.AddHours(12),
            signingCredentials: creds,
            issuer: "http://localhost:5096/",
            audience: "http://localhost:5096/"
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}