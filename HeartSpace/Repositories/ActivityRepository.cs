using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using Dapper;

namespace HeartSpace.DAL
{
	public class ActivityRepository
	{
		private readonly string _connectionString;

		public ActivityRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		// 已報名的活動
		public IEnumerable<Activity> GetRegisteredActivities(string userId)
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				string query = @"SELECT Title, Description, ImageUrl, Date, Location 
								 FROM Activities 
								 WHERE UserId = @UserId AND Status = 'Registered'";
				return db.Query<Activity>(query, new { UserId = userId });
			}
		}

		// 發起的活動
		public IEnumerable<Activity> GetInitiatedActivities(string userId)
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				string query = @"SELECT Title, Description, ImageUrl, Date, Location 
								 FROM Activities 
								 WHERE OrganizerId = @UserId";
				return db.Query<Activity>(query, new { UserId = userId });
			}
		}

		// 已發布的活動（貼文）
		public IEnumerable<Post> GetPublishedPosts(string userId)
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				string query = @"SELECT Title, Content, ImageUrl, PublishedDate 
								 FROM Posts 
								 WHERE UserId = @UserId";
				return db.Query<Post>(query, new { UserId = userId });
			}
		}

		// 已參加過的活動
		public IEnumerable<Activity> GetJoinedActivities(string userId)
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				string query = @"SELECT Title, Description, ImageUrl, Date, Location 
								 FROM Activities 
								 WHERE UserId = @UserId AND Status = 'Joined'";
				return db.Query<Activity>(query, new { UserId = userId });
			}
		}
	}

	// 資料模型
	public class Activity
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public string Date { get; set; }
		public string Location { get; set; }
	}

	public class Post
	{
		public string Title { get; set; }
		public string Content { get; set; }
		public string ImageUrl { get; set; }
		public string PublishedDate { get; set; }
	}
}