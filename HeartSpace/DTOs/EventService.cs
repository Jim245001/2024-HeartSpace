using System;
using System.Collections.Generic;
using HeartSpace.DAL;
using HeartSpace.Models.ViewModels;
using System.Linq;
using System.Data.Entity;
using HeartSpace.Models.EFModels;
using HeartSpace.Models;

namespace HeartSpace.BLL
{
	public interface IEventService
	{
		EventStatusViewModel GetEventStatus(int eventId); // 獲取活動報名狀況
	}

	public class EventService : IEventService
	{
		private readonly DAL.EventRepository _eventRepository;

		public EventService()
		{
			_eventRepository = new DAL.EventRepository();
		}

		public List<Event> GetAllEvents()
		{
			return _eventRepository.GetAllEvents();
		}

		public Event GetEventById(int id)
		{
			var eventResult = _eventRepository.GetEventById(id);


			return eventResult;

		}

		public int AddEvent(Event newEvent)
		{
			return _eventRepository.AddEvent(newEvent);
		}

		public void UpdateEvent(Event updatedEvent)
		{
			_eventRepository.UpdateEvent(updatedEvent);
		}

		public void DeleteEvent(int id)
		{
			_eventRepository.DeleteEvent(id);
		}

		// 取得所有分類
		public IEnumerable<Category> GetCategories()
		{
			return _eventRepository.GetCategories();
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


		// 取得指定活動的所有評論
		public IEnumerable<EventComment> GetEventComments(int eventId)
		{
			var comments = _eventRepository.GetEventComments(eventId);
			return comments ?? Enumerable.Empty<EventComment>();
		}


		// 新增評論
		public void AddComment(EventComment comment)
		{
			if (comment == null)
			{
				throw new ArgumentNullException(nameof(comment), "評論資料不能為空。");
			}

			if (!_eventRepository.EventExists(comment.EventId))
			{
				throw new Exception("指定的活動不存在，無法新增評論。");
			}

			_eventRepository.AddComment(comment);
		}


		// 刪除評論
		public void RemoveComment(EventComment comment)
		{
			if (comment == null)
			{
				throw new ArgumentNullException(nameof(comment), "要刪除的評論不能為空。");
			}

			_eventRepository.RemoveComment(comment);
		}


		// 檢查是否為留言者本人
		public bool IsCommentOwner(int commentId, int memberId)
		{
			return _eventRepository.IsCommentOwner(commentId, memberId);
		}

		// 更新評論
		public void UpdateComment(EventComment updatedComment)
		{
			if (updatedComment == null)
			{
				throw new ArgumentNullException(nameof(updatedComment), "更新的評論資料不能為空。");
			}

			_eventRepository.UpdateComment(updatedComment);
		}


		//檢視揪團
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

		public EventStatusViewModel GetEventStatus(int eventId)
		{
			var eventDetails = _eventRepository.GetEventWithParticipants(eventId);

			if (eventDetails == null)
			{
				return null; // 如果活動不存在
			}

			return new EventStatusViewModel
			{
				Id = eventDetails.EventId,
				EventName = eventDetails.EventName,
				EventOwner = new ParticipantViewModel
				{
					MemberId = eventDetails.MemberId,
					NickName = eventDetails.NickName,
					FullName = eventDetails.MemberName,
					Email = eventDetails.Email,
					ProfileImage = eventDetails.MemberImg
				},
				Participants = eventDetails.Participants.Select(p => new ParticipantViewModel
				{
					MemberId = p.MemberId,
					NickName = p.NickName,
					FullName = p.MemberName,
					Email = p.Email,
					ProfileImage = p.MemberImg
				}).ToList(),

				CategoryName = eventDetails.CategoryName,
				EventTime = eventDetails.EventTime,
				IsOnline = eventDetails.IsOnline,
				Location = eventDetails.Location,
				ParticipantMin = eventDetails.ParticipantMin,
				ParticipantMax = eventDetails.ParticipantMax,
				Description = eventDetails.Description,
				DeadLine = eventDetails.DeadLine,
				ParticipantNow = eventDetails.ParticipantNow
			};
		}


	}


}
