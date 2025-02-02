using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LogisticsAPI.Models;
using Microsoft.Data.Sqlite;

namespace LogisticsAPI.Controllers;

[ApiController]
[Route("api/dispatcher")]
public class DispatcherController : ControllerBase
{
    private readonly DatabaseConnection _databaseConnection = new();

    [HttpGet]
    public IActionResult GetDispatchers()
    {

        using var connection = _databaseConnection.GetConnection();
        connection.Open();

        var command = new SqliteCommand("SELECT * FROM Dispatcher", connection);
        var reader = command.ExecuteReader();

        var dispatchers = new List<dynamic>();
        while (reader.Read())
        {
            dispatchers.Add(new
            {
                Username = reader["Username"],
                CanEditDispatchers = reader["CanEditDispatchers"].Equals((long)1)
            });
        }

        return Ok(dispatchers);
    }

    [HttpGet("{username}")]
    public IActionResult GetDispatcher(string username)
    {

        using var connection = _databaseConnection.GetConnection();
        connection.Open();

        var command = new SqliteCommand("SELECT * FROM Dispatcher WHERE @Username = Username", connection);

        command.Parameters.AddWithValue("Username", username);

        var reader = command.ExecuteReader();

        var dispatchers = new List<dynamic>();
        while (reader.Read())
        {
            dispatchers.Add(new
            {
                Username = reader["Username"],
                CanEditDispatchers = reader["CanEditDispatchers"].Equals((long)1)
            });
        }

        return Ok(dispatchers);
    }


    [HttpPut("{username}")]
    public IActionResult CreateDispatcher(string username, [FromBody] DispatcherData dispatcher)
    {

        using var connection = _databaseConnection.GetConnection();
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO Dispatcher (Username, PasswordHash, CanEditDispatchers) " +
            "VALUES (@Username, @PasswordHash, @CanEditDispatchers)",
            connection);

        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash",BitConverter.ToString( SHA256.HashData(Encoding.UTF8.GetBytes(dispatcher.Password))).Replace("-",""));
        command.Parameters.AddWithValue("@CanEditDispatchers", dispatcher.CanEditDispatchers ? 1 : 0);

        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqliteException e)
        {
            if (e.SqliteErrorCode == 19)
            {
                return Conflict("Dispatcher with this name already exists");
            }

            throw e;
        }

        return NoContent();
    }

    [HttpPatch("{username}")]
    public IActionResult UpdateDispatcher(string username, [FromBody] DispatcherData dispatcher)
    {
        using var connection = _databaseConnection.GetConnection();
        connection.Open();

        var query = new StringBuilder("UPDATE Dispatcher SET ");
        var parameters = new List<SqliteParameter>();

        if (!string.IsNullOrEmpty(dispatcher.Password))
        {
            query.Append("PasswordHash = @PasswordHash, ");
            parameters.Add(new SqliteParameter("@PasswordHash", BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(dispatcher.Password))).Replace("-", "")));
        }

        if (dispatcher.CanEditDispatchers != null)
        {
            query.Append("CanEditDispatchers = @CanEditDispatchers, ");
            parameters.Add(new SqliteParameter("@CanEditDispatchers", dispatcher.CanEditDispatchers ? 1 : 0));
        }

        if (parameters.Count == 0)
            return BadRequest("No fields to update.");

        query.Length -= 2;

        query.Append(" WHERE Username = @Username");
        parameters.Add(new SqliteParameter("@Username", username));

        Console.WriteLine("query: "+ query);

        var command = new SqliteCommand(query.ToString(), connection);
        command.Parameters.AddRange(parameters.ToArray());

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
            return NotFound();

        return NoContent();
    }


    [HttpDelete("{username}")]
    public IActionResult DeleteDispatcher(string username)
    {

        using var connection = _databaseConnection.GetConnection();
        connection.Open();

        var command = new SqliteCommand("DELETE FROM Dispatcher WHERE Username = @Username", connection);
        command.Parameters.AddWithValue("@Username", username);

        var rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
            return NotFound();

        return NoContent();
    }
}
