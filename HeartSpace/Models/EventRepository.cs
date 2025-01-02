using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dapper;

namespace HeartSpace.Models
{
	public class EventRepository
	{
		private readonly string _connectionString;

		public EventRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private IDbConnection CreateConnection()
		{
			return new SqlConnection(_connectionString);
		}

		public async Task<IEnumerable<EventCard>> GetEventsAsync()
		{
			using (var connection = CreateConnection())
			{
				string sql = "SELECT Title, Organizer, Location, EventDate, ImageUrl FROM Events";
				return await connection.QueryAsync<EventCard>(sql);
			}
		}
	}
}