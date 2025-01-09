using System;
using System.Collections.Generic;
using HeartSpace.DAL;
using HeartSpace.Models.ViewModels;
using System.Linq;
using System.Data.Entity;
using HeartSpace.Models.EFModels;


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
		public EventViewModel GetEventDetailsWithExtras(int eventId, int currentMemberId)
		{
			// 從資料庫取得活動資料
			var eventEntity = _eventRepository.GetEventDetails(eventId); // 返回 EFModels.Event

			if (eventEntity == null)
			{
				return null; // 如果找不到活動，返回 null
			}

			// 將 EFModels.Event 轉換為 ViewModels.EventViewModel
			var model = new EventViewModel
			{
				Id = eventEntity.Id,
				EventName = eventEntity.EventName,
				MemberId = eventEntity.MemberId,
				ParticipantMax = eventEntity.ParticipantMax,
				ParticipantNow = _eventRepository.GetParticipantCount(eventId),
				IsOnline = eventEntity.IsOnline,
				Location = eventEntity.Location,
				Description = eventEntity.Description,
				EventTime = eventEntity.EventTime,
				DeadLine = eventEntity.DeadLine,
				CategoryName = eventEntity.Category?.CategoryName, // 假設有類別關聯
				Comments = new List<CommentViewModel>() // 初始化評論列表
			};

			// 設置是否為活動發起人
			model.IsEventOwner = model.MemberId == currentMemberId;

			// 設置是否已報名
			model.IsRegistered = _eventRepository.IsMemberRegistered(eventId, currentMemberId);

			// 加載評論資料
			var comments = _eventRepository.GetEventComments(eventId)
				.Select(c => new CommentViewModel
				{
					Id = c.Id,
					MemberId = c.MemberId,
					MemberName = c.Member?.Name ?? "未知用戶",
					MemberNickName = c.Member?.NickName ?? "未知用戶",
					MemberProfileImg = c.Member?.MemberImg,
					EventCommentContent = c.EventCommentContent,
					CommentTime = c.CommentTime
				}).ToList();
			model.Comments = comments;

			// 檢查是否已達報名上限
			model.IsFull = model.ParticipantNow >= model.ParticipantMax;

			return model;
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
					ProfileImage = p.MemberImg,
					IsAttend = p.IsAttend
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

		public void UpdateAttendance(int memberId, int eventId, bool? isAttend)
		{
			_eventRepository.UpdateAttendance(memberId, eventId, isAttend);
		}
	}


}
