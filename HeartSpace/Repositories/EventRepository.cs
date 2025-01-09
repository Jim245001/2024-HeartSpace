using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using HeartSpace.Models;
using System.Data.Entity;
using HeartSpace.Models.EFModels;
using HeartSpace.DTOs;
using System.Globalization;

namespace HeartSpace.DAL
{
	public interface IEventRepository
	{
		EventWithParticipantsDto GetEventWithParticipants(int eventId);
	}
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

		//是否為活動的擁有者
		public bool IsEventOwner(int eventId, int memberId)
		{
			using (var context = new AppDbContext())
			{
				// 確認活動是否存在，並且該活動的 MemberId 與傳入的 memberId 相符
				return context.Events.Any(e => e.Id == eventId && e.MemberId == memberId);
			}
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
		public Event GetEventDetails(int id)
		{
			using (var context = new AppDbContext())
			{
				return context.Events
					.Include(e => e.Category)
					.Include(e => e.Member) // 預先載入 Member 資料
					.FirstOrDefault(e => e.Id == id);
			}
		}


		// 新增活動
		public int AddEvent(Event newEvent)
		{
			using (var connection = CreateConnection())
			{
				const string sql = @"
INSERT INTO Events (EventName, MemberId, EventImg, CategoryId, Description, EventTime, Location, IsOnline, ParticipantMax, ParticipantMin, Limit, DeadLine, CommentCount, ParticipantNow, Disabled)
OUTPUT INSERTED.Id
VALUES (@EventName, @MemberId, @EventImg, @CategoryId, @Description, @EventTime, @Location, @IsOnline, @ParticipantMax, @ParticipantMin, @Limit, @DeadLine, @CommentCount, @ParticipantNow, @Disabled)";

				var parameters = new DynamicParameters();
				parameters.Add("@EventName", newEvent.EventName, DbType.String);
				parameters.Add("@MemberId", newEvent.MemberId, DbType.Int32);
				parameters.Add("@EventImg", newEvent.EventImg ?? (object)DBNull.Value, DbType.Binary); // 確保是二進位資料
				parameters.Add("@CategoryId", newEvent.CategoryId, DbType.Int32);
				parameters.Add("@Description", newEvent.Description, DbType.String);
				parameters.Add("@EventTime", newEvent.EventTime, DbType.DateTime);
				parameters.Add("@Location", newEvent.Location, DbType.String);
				parameters.Add("@IsOnline", newEvent.IsOnline, DbType.Boolean);
				parameters.Add("@ParticipantMax", newEvent.ParticipantMax, DbType.Int32);
				parameters.Add("@ParticipantMin", newEvent.ParticipantMin, DbType.Int32);
				parameters.Add("@Limit", newEvent.Limit, DbType.String);
				parameters.Add("@DeadLine", newEvent.DeadLine, DbType.DateTime);
				parameters.Add("@CommentCount", newEvent.CommentCount, DbType.Int32);
				parameters.Add("@ParticipantNow", newEvent.ParticipantNow, DbType.Int32);
				parameters.Add("@Disabled", newEvent.Disabled, DbType.Boolean);

				return connection.QuerySingle<int>(sql, parameters);
			}
		}


		// 更新活動
		public void UpdateEvent(Event updatedEvent)
		{
			using (var connection = CreateConnection())
			{
				const string sql = @"
					UPDATE Events
					SET EventName = @EventName, MemberId = @MemberId, EventImg = @EventImg, CategoryId = @CategoryId, Description = @Description, EventTime = @EventTime,
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
		// 根據 ID 獲取活動
		public Event GetEventById(int id)
		{
			using (var connection = CreateConnection())
			{
				const string sql = "SELECT * FROM Events WHERE Id = @Id";
				var eventResult = connection.QueryFirstOrDefault<Event>(sql, new { Id = id });


				return eventResult;

			}
		}
		// 獲取指定活動的所有評論
		public IEnumerable<EventComment> GetEventComments(int eventId)
		{
			using (var context = new AppDbContext())
			{
				// 載入指定活動的所有評論，包含 Member 資料
				return context.EventComments
					.Include(ec => ec.Member) // 確保關聯載入
					.Where(ec => ec.EventId == eventId)
					.ToList();
			}
		}

		public bool EventExists(int eventId)
		{
			using (var context = new AppDbContext())
			{
				return context.Events.Any(e => e.Id == eventId);
			}
		}

		// 新增評論資料到資料庫
		public void AddComment(EventComment comment)
		{
			using (var context = new AppDbContext())
			{
				context.EventComments.Add(comment);
				context.SaveChanges();
			}
		}

		// 刪除指定的評論
		public void RemoveComment(EventComment comment)
		{
			using (var context = new AppDbContext())
			{
				context.EventComments.Remove(comment);
				context.SaveChanges();
			}
		}


		// 確認指定會員是否為評論的擁有者
		public bool IsCommentOwner(int commentId, int memberId)
		{
			using (var context = new AppDbContext())
			{
				return context.EventComments.Any(c => c.Id == commentId && c.MemberId == memberId);
			}
		}

		// 更新指定的評論
		public void UpdateComment(EventComment updatedComment)
		{
			using (var context = new AppDbContext())
			{
				var existingComment = context.EventComments.FirstOrDefault(c => c.Id == updatedComment.Id);
				if (existingComment != null)
				{
					existingComment.EventCommentContent = updatedComment.EventCommentContent;
					context.SaveChanges();
				}
				else
				{
					throw new Exception("找不到該留言，無法更新。");
				}
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

		// 獲取活動的參與人數（包含發起人）
		public int GetParticipantCount(int eventId)
		{
			using (var connection = CreateConnection())
			{
				const string query = "SELECT COUNT(*) FROM EventMembers WHERE EventId = @EventId";
				return connection.ExecuteScalar<int>(query, new { EventId = eventId }) + 1; // +1 是加上發起人
			}
		}

		// 報名狀況頁
		public EventWithParticipantsDto GetEventWithParticipants(int eventId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				// 查詢活動的基本資訊和發起人資訊（包括類別名稱）
				var eventQuery = @"
        SELECT 
            e.Id AS EventId,
            e.EventName,
            c.CategoryName AS CategoryName, -- 從 Category 表中取得類別名稱
            e.EventTime,
            e.IsOnline,
            e.Location,
            e.ParticipantMin,
            e.ParticipantMax,
            e.Description,
            e.DeadLine,
            m.Id AS MemberId,
            m.Name AS MemberName, -- 發起人姓名
            m.NickName,
            m.Email,
            m.MemberImg,
            (SELECT COUNT(*) FROM EventMembers em WHERE em.EventId = e.Id) AS ParticipantNow
        FROM Events e
        INNER JOIN Members m ON e.MemberId = m.Id
        LEFT JOIN Categories c ON e.CategoryId = c.Id -- 連接 Categories 表
        WHERE e.Id = @EventId";

				// 查詢活動參與者清單
				var participantsQuery = @"
        SELECT 
            em.Id AS EventMemberId,
            em.EventId,
            em.MemberId,
			em.IsAttend,
            m.Name AS MemberName,
            m.NickName,
            m.Email,
            m.MemberImg
        FROM EventMembers em
        INNER JOIN Members m ON em.MemberId = m.Id
        WHERE em.EventId = @EventId";

				// 查詢活動和發起人資訊
				var eventResult = connection.QueryFirstOrDefault<EventWithParticipantsDto>(
					eventQuery,
					new { EventId = eventId }
				);

				if (eventResult == null)
				{
					return null; // 如果活動不存在，返回 null
				}

				// 查詢參與者清單
				var participants = connection.Query<ParticipantDto>(
					participantsQuery,
					new { EventId = eventId }
				).ToList();

				// 將參與者清單加入到活動 DTO 中
				eventResult.Participants = participants;

				return eventResult;
			}
		}
		// 出席狀態
		public void UpdateAttendance(int memberId, int eventId, bool? isAttend)
		{
			using (var connection = CreateConnection())
			{
				const string query = "UPDATE EventMembers SET IsAttend = @IsAttend WHERE EventId = @EventId AND MemberId = @MemberId";
				connection.Execute(query, new { MemberId = memberId, EventId = eventId, IsAttend = isAttend });
			}
		}
	}
}

