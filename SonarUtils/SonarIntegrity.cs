using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SonarUtils
{
    public static class SonarIntegrity
    {
        public static async IAsyncEnumerable<KeyValuePair<string, ImmutableArray<byte>>> GenerateHashesAsync(DirectoryInfo dirInfo, ReadOnlyMemory<byte> key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var root = dirInfo.FullName;
            var dirs = new Queue<DirectoryInfo>([dirInfo]);

            while (dirs.TryDequeue(out var dir))
            {
                foreach (var fsInfo in dir.EnumerateFileSystemInfos())
                {
                    if ((fsInfo.Attributes & FileAttributes.Directory) is FileAttributes.Directory)
                    {
                        dirs.Enqueue(new(fsInfo.FullName));
                    }
                    else
                    {
                        var file = new FileInfo(fsInfo.FullName);
                        var name = NormalizePath(Path.GetRelativePath(root, file.FullName));

                        await using var stream = file.OpenRead();
                        var hash = await SonarHashing.HMacSha256Async(stream, key, cancellationToken).ConfigureAwait(false);

                        yield return KeyValuePair.Create(name, ImmutableCollectionsMarshal.AsImmutableArray(hash));
                    }
                }
            }
        }

        public static async IAsyncEnumerable<KeyValuePair<string, ImmutableArray<byte>>> GenerateHashesAsync(ZipArchive archive, ReadOnlyMemory<byte> key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var entry in archive.Entries)
            {
                var name = NormalizePath(entry.FullName);
                if (name.EndsWith('/')) continue; // Skip directory entries

                await using var stream = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);
                var hash = await SonarHashing.HMacSha256Async(stream, key, cancellationToken).ConfigureAwait(false);

                yield return KeyValuePair.Create(name, ImmutableCollectionsMarshal.AsImmutableArray(hash));
            }
        }

        public static async IAsyncEnumerable<KeyValuePair<string, ImmutableArray<byte>>> GenerateHashesAsync(FileInfo archive, ReadOnlyMemory<byte> key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var zip = await ZipFile.OpenReadAsync(archive.FullName, cancellationToken).ConfigureAwait(false);
            await foreach (var item in GenerateHashesAsync(zip, key, cancellationToken)) yield return item;
        }

        public static async IAsyncEnumerable<KeyValuePair<string, ImmutableArray<byte>>> GenerateHashesAsync(string path, ReadOnlyMemory<byte> key, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var attributes = File.GetAttributes(path);
            var items = (attributes & FileAttributes.Directory) is FileAttributes.Directory ? GenerateHashesAsync(new DirectoryInfo(path), key, cancellationToken) : GenerateHashesAsync(new FileInfo(path), key, cancellationToken);
            await foreach (var item in items) yield return item;
        }

        private static string NormalizePath(string path)
            => path.Replace(Path.DirectorySeparatorChar, '/');

        private static string DenormalizePath(string path)
            => path.Replace('/', Path.DirectorySeparatorChar);
    }
}