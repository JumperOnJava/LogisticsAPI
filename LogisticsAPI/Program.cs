
using LogisticsAPI;
using Microsoft.Data.Sqlite;

var connection = new DatabaseConnection().GetConnection();
connection.Open();

var command = new SqliteCommand("""
CREATE TABLE IF NOT EXISTS "Dispatcher" (
    "Username" TEXT NOT NULL DEFAULT 'NULL',
    "PasswordHash" TEXT DEFAULT 'NULL',
    "CanEditDispatchers" INTEGER DEFAULT 0,
    PRIMARY KEY ("Username")
);

CREATE TABLE IF NOT EXISTS "Driver" (
    "driverId" INTEGER NOT NULL,
    "firstName" TEXT DEFAULT NULL,
    "lastName" TEXT DEFAULT NULL,
    "homeAddress" TEXT DEFAULT NULL,
    PRIMARY KEY ("driverId")
);
""", connection);
command.ExecuteNonQuery();
connection.Close();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())    
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

