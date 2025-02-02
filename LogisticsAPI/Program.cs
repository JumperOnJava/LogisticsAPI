
using LogisticsAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;

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

CreateTestAdminAccount(connection);

connection.Close(); 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = "http://localhost:5096/";
        options.Authority = "http://localhost:5096/";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey("secret-key-51067322564376754607543076524076"u8.ToArray()),
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5096/",
            ValidAudience = "http://localhost:5096/"
        };
        options.RequireHttpsMetadata = false;
    });

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

void CreateTestAdminAccount(SqliteConnection sqliteConnection)
{
    string query = "INSERT INTO Dispatcher (Username, PasswordHash, CanEditDispatchers) " +
                   "VALUES (@Username, @PasswordHash, @CanEditDispatchers)";

    var command2 = new SqliteCommand(query, sqliteConnection);
    command2.Parameters.AddWithValue("@Username", "admin");
    command2.Parameters.AddWithValue("@PasswordHash", "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918");
    command2.Parameters.AddWithValue("@CanEditDispatchers", 1);
    try
    {
        command2.ExecuteNonQuery();
    }
    catch {}
}

