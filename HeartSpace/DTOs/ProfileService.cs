using System.Collections.Generic;
using HeartSpace.DAL;

namespace HeartSpace.Business
{
	public class ProfileService
	{
		private readonly ActivityRepository _activityRepository;

		public ProfileService(ActivityRepository activityRepository)
		{
			_activityRepository = activityRepository;
		}

		public IEnumerable<Activity> GetRegisteredActivities(string userId)
		{
			return _activityRepository.GetRegisteredActivities(userId);
		}

		public IEnumerable<Activity> GetInitiatedActivities(string userId)
		{
			return _activityRepository.GetInitiatedActivities(userId);
		}

		public IEnumerable<Post> GetPublishedPosts(string userId)
		{
			return _activityRepository.GetPublishedPosts(userId);
		}

		public IEnumerable<Activity> GetJoinedActivities(string userId)
		{
			return _activityRepository.GetJoinedActivities(userId);
		}
	}
}
