using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class Statistics : IStatistics
{
    private readonly ConcurrentDictionary<string, int> _userPostCounts;
    private readonly ConcurrentDictionary<string, int> _postUpvotes;

    public Statistics()
    {
        _userPostCounts = new ConcurrentDictionary<string, int>();
        _postUpvotes = new ConcurrentDictionary<string, int>();
    }

    public void Update(IEnumerable<Post> posts)
    {
        foreach (var post in posts)
        {
            if (post.Author != null)
            {
                _userPostCounts.AddOrUpdate(post.Author, 1, (key, value) => value + 1);
            }

            if (post.Id != null)
            {
                _postUpvotes.AddOrUpdate(post.Id, post.Upvotes, (key, value) => post.Upvotes);
            }
        }
    }

    public void Report()
    {
        var topPost = _postUpvotes.OrderByDescending(x => x.Value).FirstOrDefault();
        var topUser = _userPostCounts.OrderByDescending(x => x.Value).FirstOrDefault();

        Console.WriteLine($"Top Post: {topPost.Key} with {topPost.Value} upvotes");
        Console.WriteLine($"Top User: {topUser.Key} with {topUser.Value} posts");
    }
}
