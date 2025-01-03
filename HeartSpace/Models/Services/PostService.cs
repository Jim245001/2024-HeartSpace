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
    public class PostService
    {
        private readonly PostEFRepository _repository;

        public PostService(PostEFRepository repository)
        {
            _repository = repository;
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
                PublishTime = p.PublishTime
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
                PublishTime = post.PublishTime
            };
        }

        public void AddPost(CreatePostDto dto)
        {
            var post = new Post
            {
                Title = dto.Title,
                PostContent = dto.PostContent,
                CategoryId = dto.CategoryId,
                MemberId = dto.MemberId,
                PublishTime = dto.PublishTime
            };
            _repository.AddPost(post);
        }

        public void UpdatePost(CreatePostDto dto)
        {
            var post = new Post
            {
                Id = dto.Id,
                Title = dto.Title,
                PostContent = dto.PostContent,
                CategoryId = dto.CategoryId,
                MemberId = dto.MemberId,
                PublishTime = dto.PublishTime
            };
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
    }
}