using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

using Tftp.Net;

namespace PXE_Server
{
    public class TFTPServer : IDisposable
    {
        TftpServer server;
        private static  string ServerDirectory;
        private bool disposedValue;


        public TFTPServer(IPAddress localAddress, int port,string rootDirectory)
        {
            server = new TftpServer(localAddress,port);
            ServerDirectory = rootDirectory;

            server.OnReadRequest += Server_OnReadRequest;
            server.OnWriteRequest += Server_OnWriteRequest;
            server.Start();
            Trace.WriteLine($"Start TFTPD on {localAddress.ToString()}:{port.ToString()}");
            Trace.WriteLine($"TFTP root dir: {rootDirectory}");
            Trace.Flush();
        }

        private static void Server_OnWriteRequest(ITftpTransfer transfer, System.Net.EndPoint client)
        {
            String file = Path.Combine(ServerDirectory, transfer.Filename);

            if (File.Exists(file))
            {
                CancelTransfer(transfer, TftpErrorPacket.FileAlreadyExists);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting write request from " + client);
                StartTransfer(transfer, new FileStream(file, FileMode.CreateNew));
            }
        }

        private  static void Server_OnReadRequest(ITftpTransfer transfer, System.Net.EndPoint client)
        {
            var path=Utils.CheckFileInRootDir(ServerDirectory, transfer.Filename);


            FileInfo file = new FileInfo(path);
            //Is the file within the server directory?
            if (!file.FullName.StartsWith(ServerDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                CancelTransfer(transfer, TftpErrorPacket.AccessViolation);
            }
            else if (!file.Exists)
            {
                CancelTransfer(transfer, TftpErrorPacket.FileNotFound);
            }
            else
            {
                OutputTransferStatus(transfer, "Accepting request from " + client);
                StartTransfer(transfer, new FileStream(file.FullName, FileMode.Open,FileAccess.Read,FileShare.Read));
                //StartTransfer(transfer, new MemoryStream(File.ReadAllBytes(file.FullName))); ;
            }
        }

        private static void StartTransfer(ITftpTransfer transfer, Stream stream)
        {
            transfer.OnProgress += new TftpProgressHandler(transfer_OnProgress);
            transfer.OnError += new TftpErrorHandler(transfer_OnError);
            transfer.OnFinished += new TftpEventHandler(transfer_OnFinished);
            transfer.Start(stream);
        }

        private static void CancelTransfer(ITftpTransfer transfer, TftpErrorPacket reason)
        {
            OutputTransferStatus(transfer, "Cancelling transfer: " + reason.ErrorMessage);
            transfer.Cancel(reason);
        }
        static void transfer_OnError(ITftpTransfer transfer, TftpTransferError error)
        {
            OutputTransferStatus(transfer, "Error: " + error);
        }

        static void transfer_OnFinished(ITftpTransfer transfer)
        {
            OutputTransferStatus(transfer, "Finished");
        }

        static void transfer_OnProgress(ITftpTransfer transfer, TftpTransferProgress progress)
        {
            OutputTransferStatus(transfer, "Progress " + progress);
        }

        private static void OutputTransferStatus(ITftpTransfer transfer, string message)
        {
            Trace.WriteLine("[" + transfer.Filename + "] " + message);
            Trace.Flush();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    server.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TFTPServer()
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
