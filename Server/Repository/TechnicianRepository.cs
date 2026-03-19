using Dapper;
using Server.Repository.Model;
using System.Data;

namespace Server.Repository;

public interface ITechnicianRepository
{
    Task<TechnicianAuthCredential?> FetchAuthCredential(string username);
}

public class TechnicianRepository : ITechnicianRepository
{
    private readonly IDbConnection _dbConnection;

    public TechnicianRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<TechnicianAuthCredential?> FetchAuthCredential(string username)
    {
        var credential = await _dbConnection.QuerySingleOrDefaultAsync(
            @"SELECT
                technician_id,
                password
            FROM
                sortbot.technician
            WHERE
                username = @Username",
            new
            {
                username,
            }
        );
        return credential == null ?
            null :
            new TechnicianAuthCredential()
            {
                TechnicianId = credential.technician_id,
                Username = username,
                Password = credential.password,
            };
    }

}
