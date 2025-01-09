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
                    PostImg = pm.post.PostImg.ToBase64String(),
                    PublishTime = pm.post.PublishTime,
                    MemberNickName = pm.member.NickName,
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
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .Select(post => new PostCard
                {
                    Id = post.Id,
                    Title = post.Title,
                    PostContent = post.PostContent.Length > 50 ? post.PostContent.Substring(0, 50) + "..." : post.PostContent,
                    PublishTime = post.PublishTime
                })
                .ToList();
        }
        public IEnumerable<CreatePostDto> GetAllPosts()
        {
            var posts = _repository.GetAllPosts();
            return posts.Select(p => new CreatePostDto
            {
                Id = p.Id,
                Title = p.Title,
                PostContent = p.PostContent,
                CategoryId = p.CategoryId,
                MemberId = p.MemberId,
                PublishTime = p.PublishTime,
                PostImg = p.PostImg != null ? Convert.ToBase64String(p.PostImg) : null // 將 byte[] 轉為 Base64
            });
        }

        


        public CreatePostDto GetPostById(int id)
        {
            var post = _repository.GetPostById(id);
            if (post == null) return null;

            return new CreatePostDto
            {
                Id = post.Id,
                Title = post.Title,
                PostContent = post.PostContent,
                CategoryId = post.CategoryId,
                MemberId = post.MemberId,
                PublishTime = post.PublishTime,
                PostImg = post.PostImg != null ? Convert.ToBase64String(post.PostImg) : null, // 將 byte[] 轉為 Base64
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