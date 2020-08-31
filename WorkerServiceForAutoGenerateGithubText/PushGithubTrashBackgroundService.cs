using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceForAutoGenerateGithubTrash
{
    public class PushGithubTrashBackgroundService : BackgroundService
    {
        private readonly ILogger<PushGithubTrashBackgroundService> _logger;
        private Timer _timer;
        private static Config _config;

        public PushGithubTrashBackgroundService(ILogger<PushGithubTrashBackgroundService> logger)
        {
            _logger = logger;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var jsonString = File.ReadAllText($"{baseDir}/config.json");
            _config = JsonSerializer.Deserialize<Config>(jsonString);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(3));

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("DoWork is running.");
            var directory = _config.RepositoryDirectory;

            using var repo = new Repository(directory);
            var dateTimeString = $"{DateTime.Now:yyyyMMddHHmm}";
            var fileName = $"{dateTimeString}.txt";
            Console.WriteLine(fileName);

            const string content = "Commit this!";
            File.WriteAllText(Path.Combine(repo.Info.WorkingDirectory, $"{directory}/Text/{fileName}"), content);

            Commands.Stage(repo, "*");

            // Create the committer's signature and commit
            var author = new Signature(_config.Sign.Name, _config.Sign.Email, DateTime.Now);
            var committer = author;

            // Commit to the repository
            Commit commit = repo.Commit($"Here's a commit for {dateTimeString}", author, committer);

            var options = new PushOptions();
            options.CredentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = _config.GitUser.Username,
                        Password = _config.GitUser.Password
                    });
            repo.Network.Push(repo.Branches["master"], options);
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}