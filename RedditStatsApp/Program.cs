using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var clientId = "elgE9zUp-OQf66T7zup8ew";
        var clientSecret = "tw5-rAUDmoDRpoBxsKNTRtJY9200iQ";
        var subreddit = "u/gaming";

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IRedditClient>(sp => new RedditClient(subreddit, clientId, clientSecret));
                services.AddSingleton<IStatistics, Statistics>();
                services.AddHostedService<RedditService>();
            }) 
            .Build();

        await host.RunAsync();
    }
}

public class RedditService : IHostedService
{
    private readonly IRedditClient _redditClient;
    private readonly IStatistics _statistics;
    private readonly CancellationTokenSource _cts;

    public RedditService(IRedditClient redditClient, IStatistics statistics)
    {
        _redditClient = redditClient;
        _statistics = statistics;
        _cts = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var posts = await _redditClient.GetPostsAsync();
                    _statistics.Update(posts);
                    _statistics.Report();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while fetching posts");
                }

                await Task.Delay(60000, _cts.Token); 
            }
        }, _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}
