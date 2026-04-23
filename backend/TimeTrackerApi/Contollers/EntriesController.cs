using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

[ApiController]
[Route("entries")]
[Authorize]
public class EntriesController : ControllerBase
{
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(sub))
        {
            throw new Exception("User ID claim not found in valid JWT. Check token.");
        }

        return Guid.Parse(sub);
    }

    [HttpGet("/period/{periodId}/entries")]
    public async Task<IActionResult> GetEntries(Guid periodId)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = @"
            SELECT t.id, t.date, t.hours
            FROM time_entries t
            JOIN periods p ON p.id = t.period_id
            WHERE t.period_id = @periodId AND p.user_id = @userId
            ORDER BY t.date ASC";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("periodId", periodId);
        cmd.Parameters.AddWithValue("userId", GetUserId());

        var results = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(new { id = reader["id"], date = reader["date"], hours = reader["hours"] });

        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Addentry([FromBody] EntryRequest req)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = @"
            INSERT INTO time_entries (user_id, period_id, date, hours)
            VALUES (@userId, @periodId, @date, @hours)
            RETURNING id";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", GetUserId());
        cmd.Parameters.AddWithValue("periodId", req.PeriodId);
        cmd.Parameters.AddWithValue("date", req.Date);
        cmd.Parameters.AddWithValue("hours", req.Hours);

        var newId = await cmd.ExecuteScalarAsync();
        return Ok(new { id = newId });
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntry(Guid id, [FromBody] UpdateEntryRequest req)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = "UPDATE time_entries SET date = @date, hours = @hours WHERE id= @id AND user_id = @userId";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("date", req.Date);
        cmd.Parameters.AddWithValue("hours", req.Hours);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("userId", GetUserId());

        await cmd.ExecuteNonQueryAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(Guid id)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = "DELETE FROM time_entries WHERE id = @id AND user_id = @userId";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("userId", GetUserId());

        await cmd.ExecuteNonQueryAsync();
        return NoContent();
    }

    public record EntryRequest(Guid PeriodId, DateOnly Date, decimal Hours);
    public record UpdateEntryRequest(DateOnly Date, decimal Hours);
}