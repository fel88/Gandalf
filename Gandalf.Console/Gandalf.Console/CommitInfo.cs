using LibGit2Sharp;

namespace Gandalf
{
    public class CommitInfo
    {
        public AuthorInfo Author;
        public DateTimeOffset When;
        public RepositoryInfo Rep;
        public string Sha;
        public string Message;

        public CommitInfo(RepositoryInfo repi, Commit cmt)
        {
            this.Rep = repi;
            Sha = cmt.Sha;
            When = cmt.Author.When;
            Message = cmt.Message;
            Author = new AuthorInfo() { Name = cmt.Author.Name, Email = cmt.Author.Email };

        }
    }
}
