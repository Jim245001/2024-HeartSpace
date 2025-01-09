using HeartSpace.DTOs.Services.Interfaces;
using HeartSpace.Helpers;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Repositories;
using System;
using System.Collections.Generic;
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

        public IEnumerable<CreatePostDto> FindPostsByKeyword(string keyword)
        {
            keyword = keyword?.Trim().ToLower();

            var posts = _context.Posts
                .Join(_context.Members,
                      post => post.MemberId,
                      member => member.Id,
                      (post, member) => new { post, member })
                .Where(pm =>
                    (pm.post.Title != null && pm.post.Title.ToLower().Contains(keyword)) ||
                    (pm.member.NickName != null && pm.member.NickName.ToLower().Contains(keyword)))
                .AsEnumerable()
                .Select(pm => new CreatePostDto
                {
                    Id = pm.post.Id,
                    Title = pm.post.Title,
                    PostContent = pm.post.PostContent,
                    PostImg = pm.post.PostImg != null ? Convert.ToBase64String(pm.post.PostImg) : null,
                    PublishTime = pm.post.PublishTime,
                    MemberNickName = pm.member.NickName,
                    MemberImgBase64 = pm.member.MemberImg != null ? Convert.ToBase64String(pm.member.MemberImg) : null,
                    CategoryName = _context.Categories
                        .Where(c => c.Id == pm.post.CategoryId)
                        .Select(c => c.CategoryName)
                        .FirstOrDefault(),
                    Disabled = pm.post.Disabled
                }).ToList();

            return posts;
        }
       



        public IEnumerable<PostCard> GetRandomPosts(int count)
        {
            return _context.Posts
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
           PostImg = pm.post.PostImg != null
               ? $"data:image/png;base64,{Convert.ToBase64String(pm.post.PostImg)}"
               : null,
           MemberNickName = pm.member.NickName,
           MemberImg = pm.member.MemberImg != null
               ? $"data:image/png;base64,{Convert.ToBase64String(pm.member.MemberImg)}"
               : null, // 轉換會員圖片為 Base64
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
           PostImg = pm.post.PostImg != null ? Convert.ToBase64String(pm.post.PostImg) : null,
           MemberNickName = pm.member.NickName,
           MemberImgBase64 = pm.member.MemberImg != null ? Convert.ToBase64String(pm.member.MemberImg) : null,
           CategoryName = _context.Categories
               .Where(c => c.Id == pm.post.CategoryId)
               .Select(c => c.CategoryName)
               .FirstOrDefault()
       }).ToList();

            return posts;
        }

        


        public CreatePostDto GetPostById(int id)
        {
            var postWithMember = _context.Posts
       .Join(_context.Members,
             post => post.MemberId,
             member => member.Id,
             (post, member) => new { Post = post, Member = member })
       .FirstOrDefault(pm => pm.Post.Id == id);

            if (postWithMember == null) return null;

            return new CreatePostDto
            {
                Id = postWithMember.Post.Id,
                Title = postWithMember.Post.Title,
                PostContent = postWithMember.Post.PostContent,
                PublishTime = postWithMember.Post.PublishTime,
                PostImg = postWithMember.Post.PostImg != null
                    ? Convert.ToBase64String(postWithMember.Post.PostImg)
                    : null,
                MemberNickName = postWithMember.Member.NickName,
                MemberImgBase64 = postWithMember.Member.MemberImg != null
                    ? Convert.ToBase64String(postWithMember.Member.MemberImg)
                    : null,
                CategoryName = _context.Categories
                    .Where(c => c.Id == postWithMember.Post.CategoryId)
                    .Select(c => c.CategoryName)
                    .FirstOrDefault()
            };
        }

        public int AddPost(CreatePostDto dto)
        {
            var post = new Post
            {
                Title = dto.Title,
                PostContent = dto.PostContent,
                PostImg = dto.PostImg != null ? Convert.FromBase64String(dto.PostImg) : null, // 儲存圖片
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

            post.Title = dto.Title;
            post.PostContent = dto.PostContent;
            post.CategoryId = dto.CategoryId;
            post.Disabled = dto.Disabled;

            // 如果有新圖片，更新圖片
            if (!string.IsNullOrEmpty(dto.PostImg))
            {
                post.PostImg = Convert.FromBase64String(dto.PostImg);
            }

            _repository.UpdatePost(post);
        }

        public void DeletePost(int id)
        {
            _repository.DeletePost(id);
        }

        public List<SelectListItem> GetCategories()
        {
            var categories = _repository.GetCategories();
            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(), // 下拉選單的值
                Text = c.CategoryName    // 下拉選單的顯示文字
            }).ToList();
        }

        public string GetCategoryNameById(int categoryId)
        {
            var category = _repository.GetCategories().FirstOrDefault(c => c.Id == categoryId);
            return category?.CategoryName;
        }

        public string GetMemberNameById(int memberId)
        {
            using (var db = new AppDbContext())
            {
                var member = db.Members.FirstOrDefault(m => m.Id == memberId);
                return member?.Name;
            }
        }
    }
}