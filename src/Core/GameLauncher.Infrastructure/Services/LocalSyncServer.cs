using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameLauncher.Infrastructure.Services
{
    public class LocalSyncServer : IDisposable
    {
        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private string? _currentZipPath;

        public bool IsRunning => _listener?.IsListening ?? false;
        public int Port { get; private set; } = 8080;

        public void StartServer(string zipPath, int port = 8080)
        {
            if (IsRunning) StopServer();

            _currentZipPath = zipPath;
            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{port}/");
            _listener.Start();

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ListenLoopAsync(_cts.Token), _cts.Token);
        }

        public void StopServer()
        {
            _cts?.Cancel();
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
            _listener = null;
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
            }
            catch (HttpListenerException) { /* Ignored, thrown when stopping */ }
            catch (OperationCanceledException) { /* Ignored */ }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;

            try
            {
                if (request.Url?.AbsolutePath == "/download" && !string.IsNullOrEmpty(_currentZipPath) && File.Exists(_currentZipPath))
                {
                    response.ContentType = "application/zip";
                    response.Headers.Add("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(_currentZipPath)}\"");
                    
                    using var fs = new FileStream(_currentZipPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    response.ContentLength64 = fs.Length;
                    await fs.CopyToAsync(response.OutputStream);
                }
                else if (request.Url?.AbsolutePath == "/status")
                {
                    response.ContentType = "application/json";
                    var json = "{\"status\":\"ready\",\"file\":\"" + Path.GetFileName(_currentZipPath) + "\"}";
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                System.Diagnostics.Debug.WriteLine($"Sync Server Error: {ex.Message}");
            }
            finally
            {
                response.Close();
            }
        }

        public void Dispose()
        {
            StopServer();
        }
    }
}
