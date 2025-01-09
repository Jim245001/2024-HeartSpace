using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HeartSpace.Models.ViewModels
{
	public class PostViewModels
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "���D������")]
		[StringLength(20, ErrorMessage = "���D�̦h 20 �r")]
		public string Title { get; set; }

		[Required(ErrorMessage = "���嬰����")]
		[StringLength(500, ErrorMessage = "����̦h 500 �r")]
		public string PostContent { get; set; }

		public string PostImg { get; set; } // �K��ʭ��� Base64 �榡

		public DateTime PublishTime { get; set; }
		public bool Disabled { get; set; }
		public int MemberId { get; set; }
		public string MemberNickName { get; set; } // ��PO�ʺ�

		public byte[] MemberImg { get; set; } // ��PO�Y�� Base64 �榡

		[Required(ErrorMessage = "�п�ܤ���")]
		[Display(Name = "����")]
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public IEnumerable<SelectListItem> CategoryList { get; set; }

		public List<CommentViewModel> Comments { get; set; } // �d�����X
	}
}
