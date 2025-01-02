using System.Data;
using System.Data.SqlClient;
using Dapper;
using HeartSpace.Models.EFModel;

public class MemberRepository
{
	private readonly string _connectionString = "YourConnectionStringHere";

	// 查找會員根據帳號
	public Member GetMemberByAccount(string account)
	{
		using (var connection = new SqlConnection(_connectionString))
		{
			const string sql = "SELECT * FROM Members WHERE Account = @Account";
			return connection.QueryFirstOrDefault<Member>(sql, new { Account = account });
		}
	}
}
