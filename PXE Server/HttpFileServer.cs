using System;
using System.IO;
using System.Net;

namespace PXE_Server
{
    public class HttpFileServer
    {
        private HttpListener listener;

        public int Port { get; set; } = 81;
        public string RootDirectory { get; set; }


        public HttpFileServer() : this(Path.Combine(Environment.CurrentDirectory,"wwwroot")) { }
        public HttpFileServer(string rootPath)
        {
            RootDirectory = rootPath;
        }

        public void Start()
        {
            Stop();

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + Port.ToString() + "/");
            //listener.Prefixes.Add("http://*:8181/");
            listener.Start();

            listener.BeginGetContext(OnContext, listener);


        }


        private async void OnContext(IAsyncResult ar)
        {
            var listener = (HttpListener)ar.AsyncState;
            HttpListenerContext ctx = null;
            try
            {
                 ctx = listener.EndGetContext(ar);
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 995) return;
            }

            listener.BeginGetContext(OnContext, listener);

            if (listener == null)
                return;

            var filename = ctx.Request.Url.AbsolutePath;
            filename = filename.Substring(1).Replace('/', '\\');

            filename = Path.Combine(RootDirectory, filename);
            var info = new FileInfo(filename);

            if (info.Exists)
            {
                try
                {
                    ctx.Response.ContentType = "application/octet-stream";
                    ctx.Response.ContentLength64 = info.Length;
                    ctx.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    ctx.Response.AddHeader("Last-Modified", info.LastWriteTime.ToString("r"));

                    using (var f = info.OpenRead())
                    {
                        await f.CopyToAsync(ctx.Response.OutputStream);
                    }
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    await ctx.Response.OutputStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            ctx.Response.OutputStream.Close();

        }
        
        public void Stop()
        {
            
            listener?.Stop();
            listener?.Close();
        }
    }
}
