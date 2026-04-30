using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SonarUtils
{
    public static class SonarHashing
    {
        private const int BufferSize = 4096;
        private const int KMacBits = 256;

        private static readonly ConcurrentDictionary<int, string> s_lengthExceptionMessages = new();
        private static readonly ConcurrentBag<Sha256Digest> s_sha256Bag = [];
        private static readonly ConcurrentBag<HMac> s_hmacSha256Bag = [];

        // ASSERT: Digest and mac sizes does not change
        public static readonly int Sha256Size = new Sha256Digest().GetDigestSize();
        public static readonly int HMacSha256Size = new HMac(new Sha256Digest()).GetMacSize();
        public static readonly int KMacSize = new KMac(KMacBits, "Gem was here"u8.ToArray()).GetMacSize();

        public static byte[] Sha256(ReadOnlySpan<byte> input)
        {
            var result = new byte[Sha256Size];
            Sha256(input, result);
            return result;
        }

        public static void Sha256(ReadOnlySpan<byte> input, Span<byte> output)
        {
            EnsureOutputLength(output, Sha256Size);
            var sha256 = GetOrCreateSha256();
            sha256.BlockUpdate(input);
            sha256.DoFinal(output);
            s_sha256Bag.Add(sha256);
        }

        public static byte[] Sha256(Stream input)
        {
            var result = new byte[Sha256Size];
            Sha256(input, result);
            return result;
        }

        public static void Sha256(Stream input, Span<byte> output)
        {
            EnsureOutputLength(output, Sha256Size);
            var sha256 = GetOrCreateSha256();
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    sha256.BlockUpdate(buffer.AsSpan(0, result));
                    result = input.Read(buffer);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            sha256.DoFinal(output);
            s_sha256Bag.Add(sha256);
        }

        public static async ValueTask<byte[]> Sha256Async(Stream input, CancellationToken cancellationToken = default)
        {
            var result = new byte[Sha256Size];
            await Sha256Async(input, result, cancellationToken).ConfigureAwait(false);
            return result;
        }

        public static async ValueTask Sha256Async(Stream input, Memory<byte> output, CancellationToken cancellationToken = default)
        {
            EnsureOutputLength(output.Span, Sha256Size);
            var sha256 = GetOrCreateSha256();
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    sha256.BlockUpdate(buffer.AsSpan(0, result));
                    result = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            sha256.DoFinal(output.Span);
            s_sha256Bag.Add(sha256);
        }

        public static void HMacSha256(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, Span<byte> output)
        {
            EnsureOutputLength(output, HMacSha256Size);
            var hmac = GetOrCreateHmacSha256();
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(bytes);
            hmac.DoFinal(output);
            s_hmacSha256Bag.Add(hmac);
        }

        public static byte[] HMacSha256(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key)
        {
            var result = new byte[HMacSha256Size];
            HMacSha256(bytes, key, result);
            return result;
        }

        public static void HMacSha256(Stream input, ReadOnlySpan<byte> key, Span<byte> output)
        {
            EnsureOutputLength(output, HMacSha256Size);
            var hmac = GetOrCreateHmacSha256();
            hmac.Init(new KeyParameter(key));

            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    hmac.BlockUpdate(buffer.AsSpan(0, result));
                    result = input.Read(buffer);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            hmac.DoFinal(output);
            s_hmacSha256Bag.Add(hmac);
        }

        public static byte[] HMacSha256(Stream input, ReadOnlySpan<byte> key)
        {
            var result = new byte[HMacSha256Size];
            HMacSha256(input, key, result);
            return result;
        }

        public static async ValueTask HMacSha256Async(Stream input, ReadOnlyMemory<byte> key, Memory<byte> output, CancellationToken cancellationToken = default)
        {
            EnsureOutputLength(output.Span, HMacSha256Size);
            var hmac = GetOrCreateHmacSha256();
            hmac.Init(new KeyParameter(key.Span));

            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    hmac.BlockUpdate(buffer.AsSpan(0, result));
                    result = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            hmac.DoFinal(output.Span);
            s_hmacSha256Bag.Add(hmac);
        }

        public static async ValueTask<byte[]> HMacSha256Async(Stream input, ReadOnlyMemory<byte> key, CancellationToken cancellationToken = default)
        {
            var result = new byte[HMacSha256Size];
            await HMacSha256Async(input, key, result, cancellationToken).ConfigureAwait(false);
            return result;
        }

        public static void KMac256(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, Span<byte> output, ReadOnlySpan<byte> customizationString = default)
        {
            EnsureOutputLength(output, KMacSize);
            var kmac = CreateKmac(customizationString);
            kmac.Init(new KeyParameter(key));
            kmac.BlockUpdate(bytes);
            kmac.DoFinal(output);
        }

        public static byte[] KMac256(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> customizationString = default)
        {
            var result = new byte[KMacSize];
            KMac256(bytes, key, result, customizationString);
            return result;
        }

        public static void KMac256(Stream input, ReadOnlySpan<byte> key, Span<byte> output, ReadOnlySpan<byte> customizationString = default)
        {
            EnsureOutputLength(output, KMacSize);
            var kmac = CreateKmac(customizationString);
            kmac.Init(new KeyParameter(key));

            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    kmac.BlockUpdate(buffer.AsSpan(0, result));
                    result = input.Read(buffer);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            kmac.DoFinal(output);
        }

        public static byte[] KMac256(Stream input, ReadOnlySpan<byte> key, ReadOnlySpan<byte> customizationString = default)
        {
            var result = new byte[KMacSize];
            KMac256(input, key, result, customizationString);
            return result;
        }

        public static async ValueTask KMac256Async(Stream input, ReadOnlyMemory<byte> key, Memory<byte> output, ReadOnlyMemory<byte> customizationString = default, CancellationToken cancellationToken = default)
        {
            EnsureOutputLength(output.Span, KMacSize);
            var kmac = CreateKmac(customizationString.Span);
            kmac.Init(new KeyParameter(key.Span));

            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var result = 0;
                do
                {
                    kmac.BlockUpdate(buffer.AsSpan(0, result));
                    result = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                while (result > 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            kmac.DoFinal(output.Span);
        }

        public static async ValueTask<byte[]> KMac256Async(Stream input, ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> customizationString = default, CancellationToken cancellationToken = default)
        {
            var result = new byte[KMacSize];
            await KMac256Async(input, key, result, customizationString, cancellationToken).ConfigureAwait(false);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Sha256Digest GetOrCreateSha256()
        {
            if (!s_sha256Bag.TryTake(out var digest)) digest = new();
            return digest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static HMac GetOrCreateHmacSha256()
        {
            if (!s_hmacSha256Bag.TryTake(out var signer)) signer = new(new Sha256Digest());
            return signer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static KMac CreateKmac(ReadOnlySpan<byte> customizationString)
        {
            return new(KMacBits, [.. customizationString]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureOutputLength(ReadOnlySpan<byte> output, int length)
        {
            if (output.Length < length) ThrowLengthArgumentException(length);
        }

        [DoesNotReturn]
        private static void ThrowLengthArgumentException(int length)
            => throw new ArgumentException(GetLengthArgumentExceptionMessage(length));

        private static string GetLengthArgumentExceptionMessage(int length) => s_lengthExceptionMessages.GetOrAdd(length, length => StringUtils.Intern($"output's length must be at least {length} bytes."));
    }
}
