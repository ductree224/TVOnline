using TVOnline.Models;
namespace TVOnline.Repository.Posts
{
    public interface IPostRepository
    {
        Post? FindPostById(string id);
    }
}
