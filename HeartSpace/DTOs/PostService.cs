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
                .Where(pm =>
                    (pm.post.Title != null && pm.post.Title.ToLower().Contains(keyword)) ||
                    (pm.member.NickName != null && pm.member.NickName.ToLower().Contains(keyword)))
                .OrderBy(pm => pm.post.PublishTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedPosts.Select(pm => new CreatePostDto
            {
                Id = pm.post.Id,
                Title = pm.post.Title,
                PostContent = pm.post.PostContent,
                PostImg = pm.post.PostImg,
                PublishTime = pm.post.PublishTime,
                MemberNickName = pm.member.NickName,
                MemberImg = pm.member.MemberImg,
                CategoryName = categoryDictionary.ContainsKey(pm.post.CategoryId)
                    ? categoryDictionary[pm.post.CategoryId]
                    : null,
                Disabled = pm.post.Disabled
            }).ToList();
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

            post.Title = dto.Title;
            post.PostContent = dto.PostContent;
            post.CategoryId = dto.CategoryId;
            post.Disabled = dto.Disabled;

            // 如果有新圖片，更新圖片
            if (!string.IsNullOrEmpty(dto.PostImg))
            {
                post.PostImg = dto.PostImg;
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
    }
}