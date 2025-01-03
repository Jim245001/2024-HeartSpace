﻿using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.Repositories
{
    public class PostEFRepository
    {
        private readonly AppDbContext _dbContext;

        public PostEFRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Category> GetCategories()
        {
            return _dbContext.Categories.ToList(); // 假設 Categories 是你的分類表名稱
        }

        public IEnumerable<Post> GetAllPosts()
        {
            return _dbContext.Posts.ToList();
        }

        public Post GetPostById(int id)
        {
            return _dbContext.Posts.FirstOrDefault(p => p.Id == id);
        }

        public void AddPost(Post post)
        {
            _dbContext.Posts.Add(post);
            _dbContext.SaveChanges();
        }

        public void UpdatePost(Post post)
        {
            _dbContext.Entry(post).State = System.Data.Entity.EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void DeletePost(int id)
        {
            var post = _dbContext.Posts.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                _dbContext.Posts.Remove(post);
                _dbContext.SaveChanges();
            }
        }
    }
}