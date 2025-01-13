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
using HeartSpace.DAL;
using CommentViewModel = HeartSpace.Models.ViewModels.CommentViewModel;

namespace HeartSpace.DAL
{
	public interface IEventRepository
	{
		EventWithParticipantsDto GetEventWithParticipants(int eventId);//報名狀況
		List<EventCard> SearchEvents(string keyword, int pageIndex, int pageSize);//搜尋活動
		IEnumerable<EventCard> GetRandomEvents(int count); // 取得隨機活動
	}


	public class EventRepository : IEventRepository
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

		// 取得隨機活動
		public IEnumerable<EventCard> GetRandomEvents(int count)
		{
			using (var context = new AppDbContext())
			{
				// 使用 LINQ 隨機排序並取出指定數量的活動
				var query = context.Events
					.Join(context.Categories, e => e.CategoryId, c => c.Id, (e, c) => new { e, c })
					.Join(context.Members, ec => ec.e.MemberId, m => m.Id, (ec, m) => new
					{
						ec.e.Id,
						Title = ec.e.EventName,
						EventContent = ec.e.Description,
						EventTime = ec.e.EventTime,
						EventImg = ec.e.EventImg,
						MemberNickName = m.NickName,
						MemberImg = m.MemberImg,
						CategoryName = ec.c.CategoryName
					})
					.OrderBy(x => Guid.NewGuid()) // 使用 Guid.NewGuid() 隨機排序
					.Take(count) // 取出指定數量
					.ToList();

				// 將結果轉換為 EventCard
				return query.Select(x => new EventCard(
					x.Id,
					x.Title,
					x.EventContent,
					x.EventTime,
					x.EventImg,
					x.MemberNickName,
					x.MemberImg,
					x.CategoryName
				)).ToList();
			}
		}
		//搜尋活動
		public List<EventCard> SearchEvents(string keyword, int pageIndex, int pageSize)
		{
			using (var context = new AppDbContext())
			{
				var query = context.Events
					.Join(context.Categories, e => e.CategoryId, c => c.Id, (e, c) => new { e, c })
					.Join(context.Members, ec => ec.e.MemberId, m => m.Id, (ec, m) => new
					{
						ec.e.Id,
						ec.e.EventName,
						ec.e.Description,
						ec.e.EventTime,
						ec.e.EventImg,
						MemberNickName = m.NickName,
						MemberImg = m.MemberImg,
						CategoryName = ec.c.CategoryName
					});

				if (!string.IsNullOrEmpty(keyword))
				{
					query = query.Where(x =>
						x.EventName.Contains(keyword) ||
						x.MemberNickName.Contains(keyword) ||
						x.CategoryName.Contains(keyword));
				}

				var result = query
					.OrderBy(x => x.EventTime)
					.Skip((pageIndex - 1) * pageSize)
					.Take(pageSize)
					.ToList()
					.Select(x => new EventCard(
						x.Id,
						x.EventName,
						x.Description,
						x.EventTime,
						x.EventImg,
						x.MemberNickName,
						x.MemberImg,
						x.CategoryName))
					.ToList();

				return result;
			}
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
					.Include(e => e.Member) 
					.FirstOrDefault(e => e.Id == id);
			}
		}


		// 新增活動
		public int AddEvent(Event newEvent)
		{
			using (var context = new AppDbContext())
			{
				context.Events.Add(newEvent); // 新增活動
				context.SaveChanges(); // 儲存變更
				return newEvent.Id; // 返回生成的 Id
			}
		}

		// 更新活動
		public void UpdateEvent(Event updatedEvent)
		{
			using (var context = new AppDbContext())
			{
				var existingEvent = context.Events.FirstOrDefault(e => e.Id == updatedEvent.Id);
				if (existingEvent != null)
				{
					// 更新屬性
					existingEvent.EventName = updatedEvent.EventName;
					existingEvent.MemberId = updatedEvent.MemberId;
					existingEvent.EventImg = updatedEvent.EventImg;
					existingEvent.CategoryId = updatedEvent.CategoryId;
					existingEvent.Description = updatedEvent.Description;
					existingEvent.EventTime = updatedEvent.EventTime;
					existingEvent.Location = updatedEvent.Location;
					existingEvent.IsOnline = updatedEvent.IsOnline;
					existingEvent.ParticipantMax = updatedEvent.ParticipantMax;
					existingEvent.ParticipantMin = updatedEvent.ParticipantMin;
					existingEvent.Limit = updatedEvent.Limit;
					existingEvent.DeadLine = updatedEvent.DeadLine;
					existingEvent.CommentCount = updatedEvent.CommentCount;
					existingEvent.ParticipantNow = updatedEvent.ParticipantNow;
					existingEvent.Disabled = updatedEvent.Disabled;

					// 儲存變更
					context.SaveChanges();
					// 調試輸出，確認更新後的 EventImg
					Console.WriteLine($"EventImg after update: {existingEvent.EventImg}");
				}
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
		//獲取單一評論
		public EventComment GetEventCommentById(int commentId)
		{
			using (var context = new AppDbContext())
			{
				return context.EventComments
			.Include(ec => ec.Member) // 加載關聯的 Member 資料
			.FirstOrDefault(c => c.Id == commentId);
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
				var existingComment = context.EventComments.FirstOrDefault(c => c.Id == comment.Id);
				if (existingComment != null)
				{
					existingComment.Disabled = "true"; // 更新 Disabled 欄位
					Console.WriteLine("準備保存變更到資料庫..."); // 添加日誌
					context.SaveChanges();
					Console.WriteLine("變更已保存到資料庫。"); // 添加日誌
				}
				else
				{
					throw new KeyNotFoundException("評論不存在，無法刪除。");
				}
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


