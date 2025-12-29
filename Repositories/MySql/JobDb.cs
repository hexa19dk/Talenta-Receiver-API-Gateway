using Dapper;
using MySql.Data.MySqlClient;
using TalentaReceiver.Config;

namespace TalentaReceiver.Repositories.MySql
{
    public interface IJobDb
	{
	    Task<Models.Job> Add(Models.Job o);
	    Task<Models.Job> Update(Models.Job o);
	    Task<Models.Job> Delete(Models.Job o);
	    Task<Models.Job> GetByCode(string code);
	    Task<List<Models.Job>> GetAll();
	    Task<List<Models.Job>> GetByPage(Models.Job o, int page, int pageSize, int totalRec);
	}

public class JobDb : IJobDb
{
	#region SqlCommand
	string table = "mst_job";
	string fields = "id ,code ,position ";
	string fields_insert = "@id ,@code ,@position ";
	string fields_update = "code = @code, position = @position where id = @id";

	#endregion

	private readonly IDbConnectionFactory _conFactory;
	private readonly MySqlConnection _conn;

	public JobDb(IDbConnectionFactory connectionFactory)
	{
	    _conFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
	    _conn = (MySqlConnection)_conFactory.CreateConnectionAsync().Result;
	}

	public async Task<Models.Job> Add(Models.Job o)
	{
	    var dt = DateTime.UtcNow;
	    string sqlQuery = $"insert into {table} ({fields}) values ({fields_insert})";
	    try
	    {
	        _ = await _conn.ExecuteAsync(sqlQuery, new
	        {
				id = o.Id,
				code = o.Code,
				position = o.Position
	        });
	        return o;
	    }
	    catch
	    {
	        throw;
	    }
	}
	public async Task<Models.Job> Update(Models.Job o)
	{
	    var dt = DateTime.UtcNow;
	    string sqlQuery = $"update {table} set {fields_update}";
	    try
	    {
	        _ = await _conn.ExecuteAsync(sqlQuery, new
	        {
				 id = o.Id
	        });
	        return o;
	    }
	    catch
	    {
	        throw;
	    }
	}
	public async Task<Models.Job> Delete(Models.Job o)
	{
	    var dt = DateTime.UtcNow;
	    string sqlQuery = $"delete from {table} where  id = @Id";
	    try
	    {
	        _ = await _conn.ExecuteAsync(sqlQuery, new
	        {
				 id = o.Id
	        });
	        return o;
	    }
	    catch
	    {
	        throw;
	    }
	}

	public async Task<Models.Job> GetByCode(string code)
	{
	    throw new NotImplementedException();
	}

	public async Task<List<Models.Job>> GetAll()
	{
	    throw new NotImplementedException();
	}

	public async Task<List<Models.Job>> GetByPage(Models.Job o, int page, int pageSize, int totalRec)
	{
	    throw new NotImplementedException();
	}
}
}
