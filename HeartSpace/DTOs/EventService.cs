using System;
using System.Collections.Generic;
using HeartSpace.Models.EFModel;
using HeartSpace.DAL;
using HeartSpace.Models.ViewModels;
using System.Linq;
using System.Data.Entity;
using HeartSpace.Models.EFModels;

namespace HeartSpace.BLL
{
	public class EventService
	{
		private readonly EventRepository _eventRepository;

		public EventService()
		{
			_eventRepository = new EventRepository();
		}

		public List<Event> GetAllEvents()
		{
			return _eventRepository.GetAllEvents();
		}

		public Event GetEventById(int id)
		{
			return _eventRepository.GetEventById(id);
		}

		public void AddEvent(Event newEvent)
		{
			_eventRepository.AddEvent(newEvent);
		}

		public void UpdateEvent(Event updatedEvent)
		{
			_eventRepository.UpdateEvent(updatedEvent);
		}

		public void DeleteEvent(int id)
		{
			_eventRepository.DeleteEvent(id);
		}

		// ：取得所有分類
		public IEnumerable<Category> GetCategories()
		{
			return _eventRepository.GetCategories();
		}

		// 取得指定活動的所有評論
		public IEnumerable<EventComment> GetEventComments(int eventId)
		{
			//return _eventRepository.GetEventComments(eventId);

			var comments = _eventRepository.GetEventComments(eventId);

			// 加入檢查輸出
			foreach (var comment in comments)
			{
				Console.WriteLine($"CommentId: {comment.Id}, MemberId: {comment.MemberId}, MemberName: {comment.Member?.Name ?? "未加載"}");
			}

			return comments;
		}

		// 新增評論
		public void AddComment(EventComment comment)
		{
			_eventRepository.AddComment(comment);
		}

		// 刪除評論
		public void RemoveComment(EventComment comment)
		{
			_eventRepository.RemoveComment(comment);
		}

		// 檢查是否為活動擁有者
		public bool IsEventOwner(int eventId, int memberId)
		{
			return _eventRepository.IsEventOwner(eventId, memberId);
		}

		// 檢查會員是否已報名
		public bool IsMemberRegistered(int eventId, int memberId)
		{
			return _eventRepository.IsMemberRegistered(eventId, memberId);
		}

		// 為會員註冊活動
		public void RegisterMember(int eventId, int memberId)
		{
			_eventRepository.RegisterMember(eventId, memberId);
		}

		// 取消會員的活動註冊
		public void UnregisterMember(int eventId, int memberId)
		{
			_eventRepository.UnregisterMember(eventId, memberId);
		}

		// 獲取指定活動的參與人數
		public int GetParticipantCount(int eventId)
		{
			return _eventRepository.GetParticipantCount(eventId);
		}

		public EventViewModel GetEventWithDetails(int id)
		{
			using (var context = new AppDbContext())
			{
				var eventItem = context.Events
					.Include(e => e.Category) // 確保載入 Category 資料
					.FirstOrDefault(e => e.Id == id);

				if (eventItem == null)
					return null;

				// 根據 MemberId 查詢 Member
				var member = context.Members.FirstOrDefault(m => m.Id == eventItem.MemberId);

				return new EventViewModel
				{
					Id = eventItem.Id,
					EventName = eventItem.EventName,
					MemberId = eventItem.MemberId,
					MemberName = member?.Name, // 查詢得到的 Member.Name
					MemberNickName = member?.NickName,
					MemberProfileImg = member?.MemberImg, // 查詢得到的 Member.Img
					Description = eventItem.Description,
					EventTime = eventItem.EventTime,
					Location = eventItem.Location,
					IsOnline = eventItem.IsOnline,
					ParticipantMax = eventItem.ParticipantMax,
					ParticipantMin = eventItem.ParticipantMin,
					Limit = eventItem.Limit,
					DeadLine = eventItem.DeadLine,
					CommentCount = eventItem.CommentCount,
					ParticipantNow = eventItem.ParticipantNow,
					CategoryName = eventItem.Category?.CategoryName, // 直接訪問 Category
					Disabled = eventItem.Disabled
				};
			}
		}

		// 檢查是否為留言者本人
		public bool IsCommentOwner(int commentId, int memberId)
		{
			using (var context = new AppDbContext())
			{
				// 從資料庫中查詢指定的留言
				var comment = context.EventComments.FirstOrDefault(c => c.Id == commentId);

				// 如果留言不存在，返回 false
				if (comment == null)
				{
					return false;
				}

				// 判斷該留言的 MemberId 是否等於當前用戶的 MemberId
				return comment.MemberId == memberId;
			}
		}

		public void UpdateComment(EventComment updatedComment)
		{
			using (var context = new AppDbContext())
			{
				// 查找需要更新的留言
				var existingComment = context.EventComments.FirstOrDefault(c => c.Id == updatedComment.Id);
				if (existingComment != null)
				{
					// 更新內容
					existingComment.EventCommentContent = updatedComment.EventCommentContent;

					// 保存更改
					context.SaveChanges();
				}
				else
				{
					throw new Exception("找不到該留言，無法更新。");
				}
			}
		}

	}
}
