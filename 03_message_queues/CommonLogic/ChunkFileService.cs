using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Polly;

namespace CommonLogic
{
    public class Chunk
    {
        public string FileName { get; private set; }
        public int ChunkIndex { get; private set; }
        public int TotalChunks { get; private set; }
        public byte[] Bytes { get; private set; }
        public string ParentFileHash { get; private set; }

        public Chunk(string fileName, int chunkIndex, int totalChunks, string hash, byte[] bytes)
        {
            FileName = fileName;
            ChunkIndex = chunkIndex;
            TotalChunks = totalChunks;
            Bytes = bytes;
            ParentFileHash = hash;
        }

        public static Chunk FromMessageBytes(byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);
            var parts = message.Split('|', 5);

            if (parts.Length < 5)
            {
                throw new FormatException("Invalid message format.");
            }

            return new Chunk(
                parts[0],
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                parts[3],
                Convert.FromBase64String(parts[4])
                );
        }

        public byte[] ToMessageBytes()
        {
            string metadata = $"{FileName}|{ChunkIndex}|{TotalChunks}|{ParentFileHash}";
            return Encoding.UTF8.GetBytes($"{metadata}|{Convert.ToBase64String(Bytes)}");
        }

        public override string ToString()
        {
            return $"Chunk number {ChunkIndex}/{TotalChunks} of file {FileName}.";
        }
    }

    public class ChunkFile
    {
        private const int ChunkSize = 10 * 1024;

        public Guid Guid { get; private set; }
        public byte[] Bytes { get; private set; }
        public string FileName { get; private set; }
        public string FileHash { get; private set; }
        public int FileSize => Bytes.Length;
        public int ChunksCount => (int)GetTotalChunksNumber();

        public ChunkFile(string fileName, byte[] bytes)
        {
            FileName = fileName;
            Bytes = bytes;
            FileHash = CalculateHash(bytes);
        }

        private static string CalculateHash(byte[] bytes)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        public IEnumerable<Chunk> GetChunks()
        {
            for (int i = 0; i < ChunksCount; i++)
            {
                yield return new Chunk(FileName, i + 1, ChunksCount, FileHash, GetChunk(i));
            }
        }

        private long GetTotalChunksNumber()
        {
            return (Bytes.Length + ChunkSize - 1) / ChunkSize;
        }

        private byte[] GetChunk(int chunkIndex)
        {
            int start = chunkIndex * ChunkSize;
            int remainingBytes = Bytes.Length - start;
            int currentChunkSize = Math.Min(ChunkSize, remainingBytes);

            var chunk = new byte[currentChunkSize];
            Array.Copy(Bytes, start, chunk, 0, currentChunkSize);
            return chunk;
        }
    }

    public class ChunkFileFactory
    {
        private readonly ConcurrentBag<Chunk> _receivedChunks = new();
        private int _expectedTotalChunks = 0;
        public string FileName { get; set; } = string.Empty;
        public ChunkFileFactory(string fileName)
        {
            FileName = fileName;
        }

        public async static Task<ChunkFile> CreateFromFilePathAsync(string filePath)
        {
            var retryPolicy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(1),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"[WARN] Retrying file read. Attempt {retryCount}: {exception.Message}");
                    });

            byte[] fileBytes = await retryPolicy.ExecuteAsync(async () =>
            {
                return await File.ReadAllBytesAsync(filePath);
            });

            return new ChunkFile(Path.GetFileName(filePath), fileBytes);
        }

        public void AddChunk(Chunk chunk)
        {
            if (_receivedChunks.Count == 0)
            {
                FileName = chunk.FileName;
                _expectedTotalChunks = chunk.TotalChunks;
            }
            else if (chunk.FileName != FileName)
            {
                throw new InvalidOperationException("All chunks must belong to the same file.");
            }

            _receivedChunks.Add(chunk);

            if (_receivedChunks.Count > _expectedTotalChunks)
            {
                throw new InvalidOperationException("Received more chunks than expected.");
            }
        }

        public bool CanCreateFromChunks()
        {
            return _receivedChunks.Count == _expectedTotalChunks;
        }

        public ChunkFile CreateFromChunks()
        {
            if (!CanCreateFromChunks())
            {
                throw new InvalidOperationException("Cannot create ChunkFile: not all chunks have been received.");
            }

            var sortedChunks = _receivedChunks
                .OrderBy(chunk => chunk.ChunkIndex)
                .ToList();

            var fileBytes = sortedChunks
                .SelectMany(chunk => chunk.Bytes)
                .ToArray();

            var restoredFile = new ChunkFile(FileName, fileBytes);

            string originalHash = sortedChunks.First().ParentFileHash;
            if (restoredFile.FileHash != originalHash)
            {
                throw new InvalidOperationException("File integrity check failed: Hash mismatch.");
            }

            return restoredFile;
        }
    }
}
