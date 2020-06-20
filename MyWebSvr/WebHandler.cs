using System;
using System.IO;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using PinFun.Core.Net.Http;
using PinFun.Core.ServiceHost.WebApi.Middleware;

namespace MyWebSvr
{
    class WebHandler : IWebMiddleware
    {
        public bool CanProcess(string uri)
        {
            return true;
        }

        public Task Process(HttpContext context)
        {
            var content = HtmlPageManager.Instance.GetContent(context.Request.Uri);
            if (File.Exists(content))
            {
                var response = context.Response;
                using (var fileStream = new FileStream(content, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var buffer = new byte[1024 * 1024];
                    int readCount;
                    response.SetHeader(HttpHeaderNames.CacheControl, $"max-age={60 * 60 * 24}")
                        .SetContentType("application/octet-stream")
                        .SetContentLength(fileStream.Length);
                    while ((readCount = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var sendBuffer = new byte[readCount];
                        Array.Copy(buffer, 0, sendBuffer, 0, sendBuffer.Length);
                        response.Write(sendBuffer);
                    }

                    fileStream.Close();
                }

                return response.End();
            }
            else
            {
                return context
                    .Response
                    .SetContentType("text/html; charset=utf-8")
                    .Write(content)
                    .End();
            }
        }

        public string Name => "MyWebSvr";
    }
}
