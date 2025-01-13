namespace HeartSpaceAdmin.Models
{
	public class PostViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string PostContent { get; set; }
		public string AuthorName { get; set; }
		public DateTime PublishTime { get; set; }
		public bool Disabled { get; set; }
		public List<PostCommentViewModel> Comments { get; set; } = new List<PostCommentViewModel>();
	}

	public class PostCommentViewModel
	{
		public int Id { get; set; }
		public string Comment { get; set; }
		public string UserName { get; set; }
		public DateTime CommentTime { get; set; }
		public bool? Disabled { get; set; }
	}
}
