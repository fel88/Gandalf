using LibGit2Sharp;

namespace Gandalf
{
    public class RepositoryInfo
    {
        public string Path;
        public List<ProjectInfo> Projects = new List<ProjectInfo>();
        public List<SolutionInfo> Solutions = new List<SolutionInfo>();
        public List<CommitInfo> Commits = new List<CommitInfo>();

        public CommitInfo[] UpdateCommits()
        {
            List<CommitInfo> news = new List<CommitInfo>();

            HashSet<string> hash1 = new HashSet<string>();
            foreach (var info in Commits)
            {
                hash1.Add(info.Sha);
            }
            Commits.Clear();
            using (var rep = new Repository(this.Path))
            {
                foreach (var repCommit in rep.Commits)
                {
                    var ci = new CommitInfo(this, repCommit);
                    Commits.Add(ci);
                    if (!hash1.Contains(repCommit.Sha))
                    {
                        news.Add(ci);
                    }
                }
            }

            return news.ToArray();
        }
    }
}
