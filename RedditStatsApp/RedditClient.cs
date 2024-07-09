using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RedditClient : IRedditClient
{
    private readonly RestClient _client;
    private readonly string _subreddit;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken;

    public RedditClient(string subreddit, string clientId, string clientSecret)
    {
        _client = new RestClient("https://oauth.reddit.com");
        _subreddit = subreddit;
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var authClient = new RestClient("https://www.reddit.com");
        var request = new RestRequest("/api/v1/access_token", Method.Post);
        request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}")));
        request.AddParameter("grant_type", "client_credentials");

        var response = await authClient.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            throw new Exception("Failed to retrieve access token");
        }

        var json = JObject.Parse(response.Content);
        return json["access_token"]?.ToString() ?? throw new Exception("Access token not found");
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            _accessToken = await GetAccessTokenAsync();
        }
    }

    public async Task<IEnumerable<Post>> GetPostsAsync()
    {
        await EnsureAccessTokenAsync();

        var request = new RestRequest($"/r/{_subreddit}/new.json", Method.Get);
        request.AddHeader("Authorization", $"Bearer {_accessToken}");

        var response = await _client.ExecuteAsync(request);

        if (!response.IsSuccessful || response.Content == null)
        {
            throw new Exception("Failed to retrieve posts");
        }

        var posts = new List<Post>();
        var json = JObject.Parse(response.Content);
        var items = json["data"]?["children"];

        if (items != null)
        {
            foreach (var item in items)
            {
                var data = item["data"];
                posts.Add(new Post
                {
                    Id = data?["id"]?.ToString(),
                    Author = data?["author"]?.ToString(),
                    Upvotes = data?["ups"]?.ToObject<int>() ?? 0
                });
            }
        }

        return posts;
    }
}

public class Post
{
    public string? Id { get; set; }
    public string? Author { get; set; }
    public int Upvotes { get; set; }
}
