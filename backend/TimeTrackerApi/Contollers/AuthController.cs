using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql;

[ApiController]
[Route("auth")]

public class AuthContoller : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthContoller(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        using var http = new HttpClient();
        var response = await http.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={request.IdToken}");
        if (!response.IsSuccessStatusCode)
            return Unauthorized("Invalid Google token.");

        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        if (payload is null)
            return Unauthorized("Could not read payload.");

        // This verifies that this token was issued for the time tracker app
        var expectedClientId = _config["Google:ClientId"];
        if (payload["aud"] != expectedClientId)
            return Unauthorized("Token not intended for this usage.");

        var googleId = payload["sub"];
        var email = payload["email"];
        var name = payload.GetValueOrDefault("name", "");

        var userId = await UpsertUser(googleId, email, name);

        // creating a jwt
        var token = CreateJwt(userId.ToString(), email);
        return Ok(new { token });
    }

    private async Task<Guid> UpsertUser(string googleId, string email, string name)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = @"
            INSERT INTO users (google_id, email,name)
            VALUES (@googleId, @email, @name)
            ON CONFLICT (google_id)
            DO UPDATE SET email = EXCLUDED.EMAIL, name = EXCLUDED.name 
            RETURNING id";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("googleId", googleId);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("name", name);

        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    private string CreateJwt(string userId, string email)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("email", email)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record GoogleLoginRequest(string IdToken);