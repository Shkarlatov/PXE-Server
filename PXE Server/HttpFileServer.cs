using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PXE_Server
{
    public class HttpFileServer : IDisposable
    {
        private HttpListener listener;
        private bool disposedValue;

        private int Port { get; set; } = 80;
        private string RootDirectory { get; set; }


        public HttpFileServer(int port) : this(port,Path.Combine(Environment.CurrentDirectory,"wwwroot")) { }
        public HttpFileServer(int port,string rootPath)
        {
            Port = port;
            RootDirectory = rootPath;
        }

        public void Start()
        {
            Stop();

            listener = new HttpListener();
            var prefix = "http://+:" + Port.ToString() + "/";
            listener.Prefixes.Add(prefix);
            //listener.Prefixes.Add("http://*:8181/");
            listener.Start();
            Trace.WriteLine("Start HTTPD on "+prefix);
            Trace.Flush();
            _ = DoLoop();


        }


        async Task DoLoop()
        {
            while(listener.IsListening)
            {
                var ctx=await listener.GetContextAsync();
                _=ProcessingRequest(ctx);
            }
        }

        async Task ProcessingRequest(HttpListenerContext ctx)
        {
            var filename = ctx.Request.Url.AbsolutePath;
            Trace.WriteLine("HTTP request file: "+filename);
            Trace.Flush();

            filename=Utils.CheckFileInRootDir(RootDirectory, filename);

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HttpFileServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
