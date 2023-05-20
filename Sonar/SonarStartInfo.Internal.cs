using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Sonar
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Intended")]
	public sealed partial class SonarStartInfo : ICloneable
    {
		/// <summary>Default maximum file size for reads to avoid something crazy</summary>
		private const int DefaultMaxSize = 1024 * 1024 * 1;

		internal void Initialize()
		{
			this.Locked = true;
			Directory.CreateDirectory(this.WorkingDirectory);
		}

		internal static string GetDefaultWorkingDirectory() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sonar.NET");
		private static string GetDefaultFilePath(string filename) => Path.Join(GetDefaultWorkingDirectory(), filename);

		internal string GetFilePath(string filename)
        {
			Debug.Assert(this.Locked);
			return Path.Join(this.WorkingDirectory, filename);
        }

        internal byte[]? ReadFileBytes(string filename, int maxSize = DefaultMaxSize)
		{
			Debug.Assert(this.Locked);
			var result = ReadFileBytesCore(this.GetFilePath(filename), maxSize);
			if (result is not null) return result;
			var bytes = ReadFileBytesCore(GetDefaultFilePath(filename), maxSize); // Fall-back for old versions
			if (bytes is not null) this.WriteFileBytes(filename, bytes);
			return bytes;
		}

		internal async Task<byte[]?> ReadFileBytesAsync(string filename, int maxSize = DefaultMaxSize)
		{
			Debug.Assert(this.Locked);
			var result = await ReadFileBytesCoreAsync(this.GetFilePath(filename), maxSize);
			if (result is not null) return result;
			var bytes = await ReadFileBytesCoreAsync(GetDefaultFilePath(filename), maxSize); // Fall-back for old versions
            if (bytes is not null) await this.WriteFileBytesAsync(filename, bytes);
			return bytes;
		}

		internal bool WriteFileBytes(string filename, byte[] bytes)
		{
			Debug.Assert(this.Locked);
			return WriteFileBytesCore(this.GetFilePath(filename), bytes);
		}

		internal Task<bool> WriteFileBytesAsync(string filename, byte[] bytes)
		{
			Debug.Assert(this.Locked);
			return WriteFileBytesCoreAsync(this.GetFilePath(filename), bytes);
		}

		private static byte[]? ReadFileBytesCore(string path, int maxSize)
		{
			try
			{
				using var stream = File.OpenRead(path);
				if (maxSize > 0 && stream.Length > maxSize) return null;
				var bytes = new byte[stream.Length];

				var totalRead = 0;
				while (totalRead < stream.Length)
				{
					var read = stream.Read(bytes.AsSpan(totalRead, (int)stream.Length - totalRead));
					if (read == 0) return null;
					totalRead += read;
				}
				return bytes;
			}
			catch { return null; }
		}

		private static async Task<byte[]?> ReadFileBytesCoreAsync(string path, int maxSize)
		{
			try
			{
				using var stream = File.OpenRead(path);
				if (maxSize > 0 && stream.Length > maxSize) return null;
				var bytes = new byte[stream.Length];

				var totalRead = 0;
				while (totalRead < stream.Length)
				{
					var read = await stream.ReadAsync(bytes.AsMemory(totalRead, (int)stream.Length - totalRead));
					if (read == 0) return null;
					totalRead += read;
				}
				return bytes;
			}
			catch { return null; }
		}

		private static bool WriteFileBytesCore(string path, byte[] bytes)
		{
			try
			{
				File.WriteAllBytes(path, bytes);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static async Task<bool> WriteFileBytesCoreAsync(string path, byte[] bytes)
		{
			try
			{
				await File.WriteAllBytesAsync(path, bytes);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void ThrowIfLocked()
		{
			if (this._locked) throw new InvalidOperationException($"This {nameof(SonarStartInfo)} is locked.");
		}
	}
}
