using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PinFun.Core.Utils;

namespace MyWebSvr
{
    class HtmlPageManager : Singleton<HtmlPageManager>
    {
        private readonly Dictionary<string, string> _pages = new Dictionary<string, string>();

        public Task BuildIndex(string[] paths)
        {
            _pages.Clear();
            return Task.Run(() => { _pages["/"] = BuildList("Index", paths); });
        }

        string BuildList(string title, string[] list)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge,chrome=1\" />");
            sb.AppendLine(
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no\">");
            sb.AppendLine($"<title>{title} - MyWebServer</title>");
            sb.AppendLine("</head>");
            foreach (var s in list)
            {
                var line = BuildDir("", s);
                if (string.IsNullOrWhiteSpace(line)) continue;
                sb.AppendLine(line);
            }
            sb.AppendLine("<hr style=\"height: 1px; border: none; border-top:1px solid #ccc;\" />");
            sb.AppendLine($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        string BuildDir(string parent, string dir)
        {
            if (!Directory.Exists(dir)) return null;
            var dirName = dir.Split(Path.DirectorySeparatorChar, System.StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (string.IsNullOrWhiteSpace(dirName)) return null;
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
            sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=Edge,chrome=1\" />");
            sb.AppendLine(
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no\">");
            sb.AppendLine($"<title>{parent}/{dirName} - MyWebServer</title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            if (string.IsNullOrWhiteSpace(parent))
            {
                sb.AppendLine("<div><a href='/'>返回上级</a></div>");
            }
            else
            {
                sb.AppendLine("<div><a href='/'>根</a></div>");
                sb.AppendLine($"<div><a href='{parent}'>返回上级</a></div>");
            }

            var subDirs = Directory.GetDirectories(dir);
            foreach (var subDir in subDirs)
            {
                var subDirName = subDir.Split(Path.DirectorySeparatorChar, System.StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                if (string.IsNullOrWhiteSpace(subDirName)) return null;
                var line = BuildDir($"{parent}/{dirName}", subDir);
                if (string.IsNullOrWhiteSpace(line)) continue;
                sb.AppendLine(line);
            }

            var files = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                sb.AppendLine(
                    $"<div><a href='{parent}/{dirName}/{Path.GetFileName(file)}' target='_blank'>{Path.GetFileName(file)}</a></div>");
                _pages[$"{parent}/{dirName}/{Path.GetFileName(file)}"] = file;
            }

            sb.AppendLine("<hr style=\"height: 1px; border: none; border-top:1px solid #ccc;\" />");
            sb.AppendLine($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine("</body>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            _pages[$"{parent}/{dirName}"] = sb.ToString();
            return $"<div><a href='{parent}/{dirName}'>{dirName}</a></div>";
        }

        public string GetContent(string path)
        {
            if (_pages.ContainsKey(path)) return _pages[path];
            if (_pages.ContainsKey("/")) return _pages["/"];
            return "<h1>当前没有任何工作目录被添加</h1>";
        }
    }
}
