using System.Collections.Generic;

public interface IStatistics
{
    void Update(IEnumerable<Post> posts);
    void Report();
}
