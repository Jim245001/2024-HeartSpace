using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using HeartSpace.Models.EFModel;

namespace HeartSpace.DAL
{
	public class EventRepository
	{
		private readonly string _connectionString;

		public EventRepository()
		{
			_connectionString = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;
		}

		private IDbConnection CreateConnection()
		{
			return new SqlConnection(_connectionString);
		}

		// 獲取所有活動
		public List<Event> GetAllEvents()
		{
			using (var connection = CreateConnection())
			{
				const string sql = "SELECT * FROM Events";
				return connection.Query<Event>(sql).ToList();
			}
		}

		// 根據 ID 獲取活動
		public Event GetEventById(int id)
		{
			using (var connection = CreateConnection())
			{
				const string sql = "SELECT * FROM Events WHERE Id = @Id";
				return connection.QueryFirstOrDefault<Event>(sql, new { Id = id });
			}
		}

		// 新增活動
		public void AddEvent(Event newEvent)
		{
			using (var connection = CreateConnection())
			{
				const string sql = @"
					INSERT INTO Events (EventName, MemberId, img, CategoryId, Description, EventTime, Location, IsOnline, ParticipantMax, ParticipantMin, Limit, DeadLine, CommentCount, ParticipantNow)
					VALUES (@EventName, @MemberId, @img, @CategoryId, @Description, @EventTime, @Location, @IsOnline, @ParticipantMax, @ParticipantMin, @Limit, @DeadLine, @CommentCount, @ParticipantNow)";
				connection.Execute(sql, newEvent);
			}
		}

		// 更新活動
		public void UpdateEvent(Event updatedEvent)
		{
			using (var connection = CreateConnection())
			{
				const string sql = @"
					UPDATE Events
					SET EventName = @EventName, MemberId = @MemberId, img = @img, CategoryId = @CategoryId, Description = @Description, EventTime = @EventTime,
						Location = @Location, IsOnline = @IsOnline, ParticipantMax = @ParticipantMax, ParticipantMin = @ParticipantMin,
						Limit = @Limit, DeadLine = @DeadLine, CommentCount = @CommentCount, ParticipantNow = @ParticipantNow
					WHERE Id = @Id";
				connection.Execute(sql, updatedEvent);
			}
		}

		// 刪除活動
		public void DeleteEvent(int id)
		{
			using (var connection = CreateConnection())
			{
				const string sql = "DELETE FROM Events WHERE Id = @Id";
				connection.Execute(sql, new { Id = id });
			}
		}
	}
}
