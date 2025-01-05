using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using HeartSpace.Models;
using HeartSpace.Models.EFModel;
using System.Data.Entity;

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

		// 獲取所有分類，並按照顯示順序排序
		public IEnumerable<Category> GetCategories()
		{
			using (var connection = CreateConnection())
			{
				const string query = "SELECT * FROM Categories ORDER BY DisplayOrder";
				return connection.Query<Category>(query).ToList();
			}
		}

		// 獲取指定活動的所有評論
		public IEnumerable<EventComment> GetEventComments(int eventId)
		{
			using (var context = new AppDbContext())
			{
				return context.EventComments
					.Include(ec => ec.Member) // 確保加載 Member 資料
					.Where(ec => ec.EventId == eventId)
					.ToList();
			}
		}


		// 新增評論資料到資料庫
		public void AddComment(EventComment comment)
		{
			using (var connection = CreateConnection())
			{
				const string query = @"INSERT INTO EventComments (EventId, MemberId, EventCommentContent, CommentTime)
                                        VALUES (@EventId, @MemberId, @EventCommentContent, @CommentTime)";
				connection.Execute(query, comment);
			}
		}

		// 刪除指定的評論
		public void RemoveComment(EventComment comment)
		{
			using (var connection = CreateConnection())
			{
				const string query = "DELETE FROM EventComments WHERE Id = @Id";
				connection.Execute(query, new { Id = comment.Id });
			}
		}

		// 確認指定會員是否為活動的擁有者
		public bool IsEventOwner(int eventId, int memberId)
		{
			using (var connection = CreateConnection())
			{
				const string query = "SELECT COUNT(1) FROM Events WHERE Id = @EventId AND MemberId = @MemberId";
				return connection.ExecuteScalar<bool>(query, new { EventId = eventId, MemberId = memberId });
			}
		}

		// 檢查會員是否已報名指定活動
		public bool IsMemberRegistered(int eventId, int memberId)
		{
			using (var connection = CreateConnection())
			{
				const string query = "SELECT COUNT(1) FROM EventMembers WHERE EventId = @EventId AND MemberId = @MemberId";
				return connection.ExecuteScalar<bool>(query, new { EventId = eventId, MemberId = memberId });
			}
		}

		// 為會員註冊活動
		public void RegisterMember(int eventId, int memberId)
		{
			using (var connection = CreateConnection())
			{
				const string query = @"INSERT INTO EventMembers (EventId, MemberId)
                                        VALUES (@EventId, @MemberId)";
				connection.Execute(query, new { EventId = eventId, MemberId = memberId });
			}
		}

		// 取消會員的活動註冊
		public void UnregisterMember(int eventId, int memberId)
		{
			using (var connection = CreateConnection())
			{
				const string query = "DELETE FROM EventMembers WHERE EventId = @EventId AND MemberId = @MemberId";
				connection.Execute(query, new { EventId = eventId, MemberId = memberId });
			}
		}

		// 獲取指定活動的參與人數（包含發起人）
		public int GetParticipantCount(int eventId)
		{
			using (var connection = CreateConnection())
			{
				const string query = "SELECT COUNT(*) FROM EventMembers WHERE EventId = @EventId";
				return connection.ExecuteScalar<int>(query, new { EventId = eventId }) + 1; // +1 是加上發起人
			}
		}
	}
}

