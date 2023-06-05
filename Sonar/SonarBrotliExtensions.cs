using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sonar.Numerics;
using SonarUtils;

namespace Sonar
{
    public static class SonarBrotliExtensions
    {
        /// <summary>
        /// Buffer Size cannot be any smaller than this
        /// </summary>
        private const int MinimalBufferSize = 2; // 2 is the bare minimum size

        /// <summary>
        /// Larger Buffer Size serves little purpose
        /// </summary>
        private const int MaximalBufferSize = 65536;

        /// <summary>
        /// Threshold before allocating buffer in memory
        /// </summary>
        private const int StackThreshold = 1024;

        /// <summary>
        /// Decompression buffer size multiplier
        /// </summary>
        private const int DecompressBufferSizeMultiplier = 3; // This is only a guess based upon observation of compression ratio

        /// <summary>
        /// Default Min Buffer Size
        /// </summary>
        public static int DefaultBufferSizeMin { get; set; } = MinimalBufferSize;

        /// <summary>
        /// Default Max Buffer Size
        /// </summary>
        public static int DefaultBufferSizeMax { get; set; } = MaximalBufferSize;

        public static byte[] CompressToBrotli(this byte[] src, int quality = 5, int window = 22, int bufferSize = 0)
        {
            using BrotliEncoder encoder = new(quality, window);
            if (bufferSize == 0) bufferSize = BrotliEncoder.GetMaxCompressedLength(src.Length.Clamp(MinimalBufferSize, MaximalBufferSize));
            var ret = new List<byte>();

            var srcPos = 0;
            Span<byte> dst = bufferSize > StackThreshold ? new byte[bufferSize] : stackalloc byte[bufferSize];
            while (true)
            {
                var result = encoder.Compress(src.AsSpan(srcPos), dst, out int consumed, out int written, true);
                if (result == OperationStatus.InvalidData) throw new InvalidOperationException("Invalid Data");
                srcPos += consumed;
                if (result == OperationStatus.NeedMoreData && srcPos == src.Length) throw new InvalidOperationException("Need More Data");
                ret.AddRange(dst[..written]);
                if (result == OperationStatus.Done) break;
            }
            return ret.ToArray();
        }

        public static async Task CompressToBrotliAsync(Stream outStream, Stream inStream, int quality = 5, int window = 22, int bufferSize = 0, CancellationToken token = default)
        {
            using BrotliEncoder encoder = new(quality, window);
            if (bufferSize == 0) bufferSize = BrotliEncoder.GetMaxCompressedLength(((int)inStream.Length).Clamp(MinimalBufferSize, MaximalBufferSize));

            Memory<byte> src = new byte[bufferSize];
            Memory<byte> dst = new byte[bufferSize];

            int bytesRead = await inStream.ReadAsync(src, token);
            while (bytesRead > 0)
            {
                int srcPos = 0;
                while (srcPos < bytesRead)
                {
                    var result = encoder.Compress(src[srcPos..bytesRead], dst, out int consumed, out int written, false);
                    if (result == OperationStatus.InvalidData) throw new InvalidOperationException("Invalid Data");
                    srcPos += consumed;
                    await outStream.WriteAsync(dst[..written], token);
                    if (result == OperationStatus.Done) break;
                }
                bytesRead = await inStream.ReadAsync(src, token);
            }

            while (true)
            {
                var result = encoder.Flush(dst, out int written);
                await outStream.WriteAsync(dst[..written], token);
                if (result == OperationStatus.Done) break;
            }
        }

        public static byte[] DecompressFromBrotli(this byte[] src, int bufferSize = 0)
        {
            using BrotliDecoder decoder = new();
            if (bufferSize == 0) bufferSize = BrotliEncoder.GetMaxCompressedLength((src.Length * DecompressBufferSizeMultiplier).Clamp(MinimalBufferSize, MaximalBufferSize));
            var ret = new List<byte>();

            var srcPos = 0;
            Span<byte> dst = bufferSize > StackThreshold ? new byte[bufferSize] : stackalloc byte[bufferSize];
            while (true)
            {
                var result = decoder.Decompress(src.AsSpan(srcPos), dst, out int consumed, out int written);
                if (result == OperationStatus.InvalidData) throw new InvalidOperationException("Invalid Data");
                srcPos += consumed;
                if (result == OperationStatus.NeedMoreData && srcPos == src.Length) throw new InvalidOperationException("Need More Data");
                ret.AddRange(dst[..written]);
                if (result == OperationStatus.Done) break;
            }
            return ret.ToArray();
        }

        public static async Task DecompressToBrotliAsync(Stream outStream, Stream inStream, int bufferSize = 0, CancellationToken token = default)
        {
            using BrotliDecoder decoder = new();
            if (bufferSize == 0) bufferSize = BrotliEncoder.GetMaxCompressedLength(((int)inStream.Length * DecompressBufferSizeMultiplier).Clamp(MinimalBufferSize, MaximalBufferSize));

            Memory<byte> src = new byte[bufferSize];
            Memory<byte> dst = new byte[bufferSize];

            int bytesRead = await inStream.ReadAsync(src, token);
            OperationStatus result = default;
            while (bytesRead > 0)
            {
                int srcPos = 0;
                while (srcPos < bytesRead)
                {
                    result = decoder.Decompress(src[srcPos..bytesRead], dst, out int consumed, out int written);
                    if (result == OperationStatus.InvalidData) throw new InvalidOperationException("Invalid Data");
                    srcPos += consumed;
                    await outStream.WriteAsync(dst[..written], token);
                    if (result == OperationStatus.Done) break;
                }
                bytesRead = await inStream.ReadAsync(src, token);
            }
            if (result == OperationStatus.NeedMoreData) throw new InvalidOperationException("Need more data");
        }

        public static OperationStatus Compress(this BrotliEncoder encoder, Memory<byte> source, Memory<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
        {
            return encoder.Compress(source.Span, destination.Span, out bytesConsumed, out bytesWritten, isFinalBlock);
        }
        public static OperationStatus Decompress(this BrotliDecoder decoder, Memory<byte> source, Memory<byte> destination, out int bytesConsumed, out int bytesWritten)
        {
            return decoder.Decompress(source.Span, destination.Span, out bytesConsumed, out bytesWritten);
        }
        public static OperationStatus Flush(this BrotliEncoder encoder, Memory<byte> destination, out int bytesWritten)
        {
            return encoder.Flush(destination.Span, out bytesWritten);
        }
    }
}
