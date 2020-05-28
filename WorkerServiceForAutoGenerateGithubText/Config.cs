namespace WorkerServiceForAutoGenerateGithubTrash
{
    public class Config
    {
        public string RepositoryDirectory { get; set; }
        public Sign Sign { get; set; }
        public GitUser GitUser { get; set; }
    }

    public class Sign
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class GitUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}