using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

[ApiController]
[Route("periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetPeriods()
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = @"
            SELECT p.id, p.label, p.start_date, p.end_date, COALESCE(SUM(t.hours), 0) AS total_hours
            FROM periods p
            LEFT JOIN time_entries t on t.period_id = p.id
            WHERE p.user_id = @userId
            GROUP BY p.id
            ORDER BY p.start_date DESC";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", GetUserId());

        var results = new List<object>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new
            {
                id = reader["id"],
                label = reader["label"],
                startDate = reader["start_date"],
                endDate = reader["end_date"],
                totalHours = reader["total_hours"]
            });
        }
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePeriod([FromBody] CreatePeriodRequest req)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = @"
            INSERT INTO periods (user_id, label, start_date, end_date)
            VALUES (@userId, @label, @startDate, @endDate)
            RETURNING id";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", GetUserId());
        cmd.Parameters.AddWithValue("label", req.Label);
        cmd.Parameters.AddWithValue("startDate", req.StartDate);
        cmd.Parameters.AddWithValue("endDate", req.EndDate);

        var newId = await cmd.ExecuteScalarAsync();
        return Ok(new { id = newId });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePeriod(Guid id)
    {
        await using var conn = Db.CreateConnection();
        await conn.OpenAsync();

        var sql = "DELETE FROM periods WHERE id = @id AND user_id = @userId";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("userId", GetUserId());

        await cmd.ExecuteNonQueryAsync();
        return NoContent();
    }
}

public record CreatePeriodRequest(string Label, DateOnly StartDate, DateOnly EndDate);