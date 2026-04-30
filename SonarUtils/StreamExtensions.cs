using Org.BouncyCastle.Asn1.X509;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class StreamExtensions
    {
        extension(Stream stream)
        {
            public async Task<MemoryStream?> ToMemoryStreamAsync(int bytesLimit = -1, CancellationToken cancellationToken = default)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(4096);
                var memoryStream = new MemoryStream();
                try
                {
                    var totalBytes = 0;
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                        if (bytesRead is 0)
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return memoryStream;
                        }
                        totalBytes += bytesRead;
                        if (bytesLimit is not -1 && totalBytes > bytesLimit) return null;
                        await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            public Task<MemoryStream?> ToMemoryStreamAsync(CancellationToken cancellationToken) => stream.ToMemoryStreamAsync(-1, cancellationToken);

            public async Task<int> GetLengthSlowAsync(CancellationToken cancellationToken = default)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(4096);
                try
                {
                    var totalBytes = 0;
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                        if (bytesRead is 0) return totalBytes;
                        totalBytes += bytesRead;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
