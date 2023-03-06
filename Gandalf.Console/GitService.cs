using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Diagnostics;
using System.Text;

namespace Gandalf
{
    public class GitService
    {
        public string Username;
        public string Password;
        public string Email;

        public static string ExecuteGitBashCommand(string fileName, string command, string workingDir)
        {

            ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName, "-c \" " + command + " \"")
            {
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            var process = new Process();
            processStartInfo.FileName = "git";
            processStartInfo.Arguments = command;
            process.StartInfo = processStartInfo;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            var exitCode = process.ExitCode;
            process.WaitForExit();

            process.Close();

            return output + Environment.NewLine + error;
        }
        public static string GitBashPath = "C:\\Program Files\\Git\\git-bash.exe";
        public string GitExec(string command, string workdir)
        {
            return ExecuteGitBashCommand(GitBashPath, command, workdir);
        }

        public void Checkout(CommitInfo cmt)
        {

            using (var r = new Repository(cmt.Rep.Path))
            {
                var cm = r.Commits.First(z => z.Sha == cmt.Sha);
                LibGit2Sharp.Commands.Checkout(r, cm);

            }
        }


        public void Pull(RepositoryInfo rep)
        {
            using (var r = new Repository(rep.Path))
            {
                LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = Username,
                            Password = Password
                        });

                // User information to create a merge commit
                var signature = new LibGit2Sharp.Signature(
                    new Identity(Username, Email), DateTimeOffset.Now);

                // Pull
                LibGit2Sharp.Commands.Pull(r, signature, options);

            }
        }
    }
}
