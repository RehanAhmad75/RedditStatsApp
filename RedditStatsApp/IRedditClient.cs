using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRedditClient
{
    Task<IEnumerable<Post>> GetPostsAsync();
}
