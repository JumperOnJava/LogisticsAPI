using LogisticsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace LogisticsAPI.Controllers;

[ApiController]
[Route("api/driver")]
public class DriversController : ControllerBase
{
    private readonly DatabaseConnection _dbConnection = new();

    [HttpGet]
    public async Task<IActionResult> GetAllDrivers()
    {
        var drivers = new List<DriverOutput>();
        var query = "SELECT driverId, firstName, lastName, homeAddress FROM Driver";

        await using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();
        await using var command = new SqliteCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            drivers.Add(new DriverOutput
            {
                DriverId = reader.GetInt32(0),
                FirstName = reader.IsDBNull(1) ? null : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                HomeAddress = reader.IsDBNull(3) ? null : reader.GetString(3)
            });

        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriver(int id)
    {
        DriverOutput driverInput = null;
        var query = "SELECT driverId, firstName, lastName, homeAddress FROM Driver WHERE driverId = @id";

        await using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();
        await using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            driverInput = new DriverOutput
            {
                DriverId = reader.GetInt32(0),
                FirstName = reader.IsDBNull(1) ? null : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                HomeAddress = reader.IsDBNull(3) ? null : reader.GetString(3),
            };

        return driverInput != null ? Ok(driverInput) : NotFound();
    }

    [HttpPut]
    public async Task<IActionResult> CreateDriver([FromBody] DriverInput driverInput)
    {
        await using var connection = _dbConnection.GetConnection();
        connection.Open();

        var insertQuery = @"
            INSERT INTO Driver (firstName, lastName, homeAddress)
            VALUES (@firstName, @lastName, @homeAddress);
            SELECT last_insert_rowid();";
        await using var insertCommand = new SqliteCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@firstName", driverInput.FirstName ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@lastName", driverInput.LastName ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@homeAddress", driverInput.HomeAddress ?? (object)DBNull.Value);

        var newDriverId = Convert.ToInt32(insertCommand.ExecuteScalar());

        return CreatedAtAction(nameof(GetDriver), new { id = newDriverId }, driverInput.Identified(newDriverId));
    }



    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDriver(int id, [FromBody] DriverInput driverInput)
    {
        var query = @"
        UPDATE Driver 
        SET firstName = @firstName, lastName = @lastName, homeAddress = @homeAddress 
        WHERE driverId = @id";

        await using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();
        await using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@firstName", (object?)driverInput.FirstName ?? DBNull.Value);
        command.Parameters.AddWithValue("@lastName", (object?)driverInput.LastName ?? DBNull.Value);
        command.Parameters.AddWithValue("@homeAddress", (object?)driverInput.HomeAddress ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0) return NotFound();

        return Ok(driverInput.Identified(id));
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        var query = "DELETE FROM Driver WHERE driverId = @id";

        await using var connection = _dbConnection.GetConnection();
        await connection.OpenAsync();
        await using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0) return NotFound();

        return Ok();
    }
}