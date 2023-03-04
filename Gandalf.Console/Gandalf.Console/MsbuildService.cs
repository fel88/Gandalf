using System.Text;
using System.Diagnostics;
using Telegram.Bot.Requests;

namespace Gandalf
{
    public class MsbuildService
    {
        public string MsBuildPath;
        public List<FileInfo> GetAllFiles(DirectoryInfo dir, List<FileInfo> list)
        {
            if (list == null)
            {
                list = new List<FileInfo>();
            }
            foreach (var directoryInfo in dir.GetDirectories())
            {
                GetAllFiles(directoryInfo, list);
            }
            list.AddRange(dir.GetFiles());
            return list;
        }

        public StringBuilder NugetRestore(CommitInfo cmt)
        {
            StringBuilder log = new StringBuilder();
            Process process = new Process();
            process.StartInfo.FileName = "nuget.exe";

            var dd = new DirectoryInfo(cmt.Rep.Path);
            var list = GetAllFiles(dd, null);
            var fr = list.First(z => z.Name.Contains(".sln"));
            process.StartInfo.Arguments = "restore \"" + fr.DirectoryName + "\"";
            var comb2 = Path.Combine(cmt.Rep.Path, cmt.Rep.Solutions.First().Path);
            var finf = new FileInfo(comb2);
            process.StartInfo.Arguments = "restore \"" + finf.DirectoryName + "\"";



            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("cp866");
            process.Start();



            string output = process.StandardOutput.ReadToEnd();
            log.Append(output);
            return log;
        }

        public StringBuilder Build(RepositoryInfo rep, GitService gs)
        {
            gs.Pull(rep);
            rep.UpdateCommits();

            var cmt = rep.Commits.First();
            return Build(cmt);
        }

        public StringBuilder Build(CommitInfo cmt)
        {
            StringBuilder log = new StringBuilder();
            var cmb = Path.Combine(cmt.Rep.Path, cmt.Rep.Projects.First().Path);
            //File.WriteAllLines("temp.bat", new string[] { "cd \""+ MsBuildPath+"\"", MsBuildPath2, "msbuild \"" + cmb+"\"" });
            Process process = new Process();
            process.StartInfo.FileName = MsBuildPath;
            //process.StartInfo.FileName = "cmd.exe";

            //process.StartInfo.WorkingDirectory = AssemblyDirectory;
            process.StartInfo.Arguments = "/t:Rebuild  \"" + cmb + "\"";
            //process.StartInfo.FileName = "temp.bat";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("cp866");
            process.Start();
            //* Read the output (or the error)
            log.Clear();


            //string err = process.StandardError.ReadToEnd();
            //Console.WriteLine(err);

            //process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            var spl = output.Split(new char[] { '\n' }).ToArray();
            foreach (var s in spl)
            {
                var low = s.ToLower();
                if (!low.Contains("errorreport") && (low.Contains("error") || low.Contains("ошибка") || low.Contains("ошибок")))
                {
                    log.AppendLine(low);
                }
            }

            /*Protocol.Items.Add(new ProtocolItem()
            {
                Timestamp = DateTime.Now,
                Commit = cmt,
                Result = spl.Any(z => z.ToLower().Contains("ошибок: 0")) ? ProtocolItemResultEnum.Success : ProtocolItemResultEnum.Error,
                Output *= output
            });*/
            //Console.WriteLine(output);
            //richTextBox1.AppendText(output);

            //psi.Arguments =


            //Process.Start(psi);
            while (!process.HasExited)
            {

            }
            process.Close();
            return log;
        }
    }
}
