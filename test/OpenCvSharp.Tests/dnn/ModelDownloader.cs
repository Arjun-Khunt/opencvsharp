﻿using System;
using System.IO;
using Xunit.Abstractions;

namespace OpenCvSharp.Tests.Dnn
{
    internal static class ModelDownloader
    {
        private static readonly object lockObj = new object();

        /// <summary>
        /// Download model file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        public static byte[] Download(Uri uri, string fileName)
        {
            lock (lockObj)
            {
                int beforePercent = 0;
                var contents = DownloadBytes(uri, progress =>
                {
                    if (progress.ProgressPercentage == beforePercent)
                        return;
                    beforePercent = progress.ProgressPercentage;
#if DEBUG
                    Console.WriteLine("[{0}] Download Progress: {1}/{2} ({3}%)",
                        fileName,
                        progress.BytesReceived,
                        progress.TotalBytesToReceive,
                        progress.ProgressPercentage);
#endif
                });
                return contents;
            }
        }

        /// <summary>
        /// Download model file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        public static void DownloadAndSave(Uri uri, string fileName)
        {
            if (File.Exists(fileName))
                return;

            var bytes = Download(uri, fileName);
            File.WriteAllBytes(fileName, bytes);
        }

        private static byte[] DownloadBytes(
            Uri uri,
            Action<(long BytesReceived, long TotalBytesToReceive, int ProgressPercentage)>? downloadProgressChangedEvent = null)
        {
            using var client = new MyWebClient();
            if (downloadProgressChangedEvent == null)
            {
                return client.DownloadData(uri);
            }

            var task = client.DownloadDataTaskAsync(
                uri,
                new Progress<(long BytesReceived, long TotalBytesToReceive, int ProgressPercentage)>(downloadProgressChangedEvent));
            return task.Result;
            //var response = (httpClient.GetAsync(uri).Result).EnsureSuccessStatusCode();
            //return response.Content.ReadAsByteArrayAsync().Result;
        }
    }
}