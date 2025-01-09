using HeartSpace.Models;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartSpace.DTOs.Services.Interfaces
{
    public interface IPostService
    {
        IEnumerable<CreatePostDto> GetAllPosts();
        IEnumerable<CreatePostDto> FindPostsByKeyword(string keyword);

        IEnumerable<PostCard> GetRandomPosts(int count);

    }
}
