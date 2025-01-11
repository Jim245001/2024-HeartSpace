using HeartSpace.DTOs.Services.Interfaces;
using HeartSpace.Helpers;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Models.Services
{
    public class PostService : IPostService
    {
        private readonly PostEFRepository _repository;

        private readonly AppDbContext _context;

        public PostService(PostEFRepository repository, AppDbContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<CreatePostDto> FindPostsByKeyword(string keyword, int pageIndex, int pageSize)
        {
            var categoryDictionary = _context.Categories
        .ToDictionary(c => c.Id, c => c.CategoryName);

			var pagedPosts = _context.Posts
		        .Join(_context.Members,
			          post => post.MemberId,
			          member => member.Id,
			          (post, member) => new { post, member })
		        .Join(_context.Categories,
			          pm => pm.post.CategoryId,
			          category => category.Id,
			          (pm, category) => new { pm.post, pm.member, category }) // 包含分類
		        .Where(pmc =>
			        (pmc.post.Title != null && pmc.post.Title.ToLower().Contains(keyword)) || // 搜尋標題
			        (pmc.member.NickName != null && pmc.member.NickName.ToLower().Contains(keyword)) || // 搜尋暱稱
			        (pmc.category.CategoryName != null && pmc.category.CategoryName.ToLower().Contains(keyword))) // 搜尋分類名稱
		        .OrderBy(pmc => pmc.post.PublishTime)
		        .Skip((pageIndex - 1) * pageSize)
		        .Take(pageSize)
		        .ToList();

			return pagedPosts.Select(pmc => new CreatePostDto
			{
				Id = pmc.post.Id,
				Title = pmc.post.Title,
				PostContent = pmc.post.PostContent,
				PostImg = pmc.post.PostImg,
				PublishTime = pmc.post.PublishTime,
				MemberNickName = pmc.member.NickName,
				MemberImg = pmc.member.MemberImg,
				CategoryName = pmc.category.CategoryName, 
				Disabled = pmc.post.Disabled
			}).ToList();
		}





        public IEnumerable<PostCard> GetRandomPosts(int count)
        {
            return _context.Posts
                .Where(post => !post.Disabled)
       .Join(_context.Members,
             post => post.MemberId,
             member => member.Id,
             (post, member) => new { post, member })
       .OrderBy(pm => Guid.NewGuid())
       .Take(count)
       .AsEnumerable()
       .Select(pm => new PostCard
       {
           Id = pm.post.Id,
           Title = pm.post.Title,
           PostContent = pm.post.PostContent.Length > 50
               ? pm.post.PostContent.Substring(0, 50) + "..."
               : pm.post.PostContent,
           PublishTime = pm.post.PublishTime,
           PostImg = pm.post.PostImg,
           MemberNickName = pm.member.NickName,
           MemberImg = pm.member.MemberImg, 
           CategoryName = _context.Categories
               .Where(c => c.Id == pm.post.CategoryId)
               .Select(c => c.CategoryName)
               .FirstOrDefault()
       })
       .ToList();
        }
        public IEnumerable<CreatePostDto> GetAllPosts()
        {
            var posts = _context.Posts
       .Join(_context.Members,
             post => post.MemberId,
             member => member.Id,
             (post, member) => new { post, member })
       .AsEnumerable()
       .Select(pm => new CreatePostDto
       {
           Id = pm.post.Id,
           Title = pm.post.Title,
           PostContent = pm.post.PostContent,
           PublishTime = pm.post.PublishTime,
           PostImg = pm.post.PostImg,
           CategoryName = _context.Categories
               .Where(c => c.Id == pm.post.CategoryId)
               .Select(c => c.CategoryName)
               .FirstOrDefault()
       }).ToList();

            return posts;
        }

        


        public CreatePostDto GetPostById( int id)
        {
            var postWithMember = _context.Posts
       .Join(_context.Members,
             post => post.MemberId,
             member => member.Id,
             (post, member) => new { Post = post, Member = member })
       .FirstOrDefault(pm => pm.Post.Id == id);

            var categoryId = postWithMember.Post.CategoryId;

            if (categoryId <= 0)
            {
                throw new Exception($"Invalid CategoryId: {categoryId}. Please check data integrity.");
            }

            var categoryName = _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault();

            if (postWithMember == null) return null;

            return new CreatePostDto
            {
                Id = postWithMember.Post.Id,
                Title = postWithMember.Post.Title,
                PostContent = postWithMember.Post.PostContent,
                PublishTime = postWithMember.Post.PublishTime,
                PostImg = postWithMember.Post.PostImg,
                MemberNickName = postWithMember.Member.NickName,
                MemberImg = postWithMember.Member.MemberImg,
                Disabled = postWithMember.Post.Disabled,
                CategoryName = categoryName,
                CategoryId = postWithMember.Post.CategoryId,
                MemberId = postWithMember.Post.MemberId != 0
            ? postWithMember.Post.MemberId // 如果有正確的 MemberId 就用它
            : 1 // 

            };


        }

        public int AddPost(CreatePostDto dto)
        {
            var post = new Post
            {
                Title = dto.Title,
                PostContent = dto.PostContent,
                PostImg = dto.PostImg, 
                PublishTime = dto.PublishTime,
                CategoryId = dto.CategoryId,
                MemberId = dto.MemberId
            };

            _repository.AddPost(post);

            // 確保回傳新增貼文的 ID
            return post.Id;
        }

        public void UpdatePost(CreatePostDto dto)
        {

            var post = _repository.GetPostById(dto.Id);
            if (post == null)
            {
                throw new Exception("找不到該貼文！");
            }

            // 更新基本資料
            post.Title = dto.Title;
            post.PostContent = dto.PostContent;
            post.CategoryId = dto.CategoryId;
            post.Disabled = dto.Disabled;

            // 更新圖片路徑（無論是新圖片還是清空圖片）
            post.PostImg = dto.PostImg;

            _repository.UpdatePost(post);
        }

        public void DeletePost(int id)
        {
            _repository.DeletePost(id);
        }

        public List<SelectListItem> GetCategories()
        {
            var categories = _repository.GetCategories();

            if (categories == null || !categories.Any())
            {
                throw new Exception("無法從資料庫中取得分類資料！");
            }
            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(), // 下拉選單的值
                Text = c.CategoryName    // 下拉選單的顯示文字
            }).ToList();
        }

        public string GetCategoryNameById(int categoryId)
        {
          
            var category = _repository.GetCategories().FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
            {
                throw new Exception($"找不到對應的類別，CategoryId: {categoryId}");
            }

            return category.CategoryName;
        }

        public string GetMemberNameById(int memberId)
        {
            using (var db = new AppDbContext())
            {
                var member = db.Members.FirstOrDefault(m => m.Id == memberId);
                return member?.Name;
            }
        }

        public IQueryable<PostCard> SearchPosts(string keyword)
        {
            return _context.Posts
        .Where(p => p.Title.Contains(keyword) || p.PostContent.Contains(keyword))
        .Select(p => new PostCard
        {
            Id = p.Id,
            Title = p.Title,
            CategoryName = _context.Categories
                .Where(c => c.Id == p.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefault(),
            PublishTime = p.PublishTime,
            MemberNickName = p.Member.NickName,
            PostImg = p.PostImg
        });
        }
    }
    
}