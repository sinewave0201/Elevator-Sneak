using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.InferenceEngine.Samples.TTS.Utils
{
    public class HfDownloader
    {
        const string k_BaseUrl = "https://huggingface.co/";
        readonly string m_RepoId;
        readonly string m_Revision;
        readonly string m_DownloadPath;
        readonly SynchronizationContext m_UnityContext;

        public string DownloadPath => m_DownloadPath;

        /// <param name="downloadPath">The local path to store the downloaded files</param>
        /// <param name="repoId">The repo id on hf</param>
        /// <param name="revision">The branch name</param>
        public HfDownloader(string downloadPath, string repoId, string revision = "main")
        {
            m_DownloadPath = downloadPath;
            m_RepoId = repoId;
            m_Revision = revision;

            // Capture Unity's main thread SynchronizationContext at construction time
            m_UnityContext = SynchronizationContext.Current
                             ?? throw new InvalidOperationException("No SynchronizationContext found. Must be created on Unity's main thread.");
        }

        /// <summary>
        /// Download a file from the specified Hugging Face repo and revision.
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="progress">Progress of download, value [0, 1]</param>
        public async Task Download(string filePath, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var url = GetUrl(m_RepoId, m_Revision, filePath);
            using var client = new HttpClient();

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync();

            var destinationPath = Path.Join(m_DownloadPath, filePath);
            var directoryPath = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var temporaryPath = destinationPath + ".download";
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);

            try
            {
                using var fileStream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        var progressValue = (float)totalRead / totalBytes;
                        m_UnityContext.Post(_ => progress.Report(progressValue), null);
                    }
                }

                fileStream.Close();
                File.Move(temporaryPath, destinationPath);
                m_UnityContext.Post(_ => progress?.Report(1f), null);
            }
            catch
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
                throw;
            }
        }

        static string GetUrl(string repoId, string revision, string filePath)
        {
            return $"{k_BaseUrl}{repoId}/resolve/{revision}/{filePath}";
        }
    }
}
