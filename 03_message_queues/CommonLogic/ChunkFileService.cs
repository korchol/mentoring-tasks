using System.Collections.Concurrent;
using System.Text;

namespace CommonLogic
{
    public class Chunk
    {
        public string FileName { get; private set; }
        public int ChunkIndex { get; private set; }
        public int TotalChunks { get; private set; }
        public byte[] Bytes { get; private set; }


        public Chunk(string fileName, int chunkIndex, int totalChunks, byte[] bytes)
        {
            FileName = fileName;
            ChunkIndex = chunkIndex;
            TotalChunks = totalChunks;
            Bytes = bytes;
        }

        public static Chunk FromMessageBytes(byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);
            var parts = message.Split('|', 4);

            if (parts.Length < 4)
            {
                throw new FormatException("Invalid message format.");
            }

            return new Chunk(
                parts[0],
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                Convert.FromBase64String(parts[3])
                );
        }

        public byte[] ToMessageBytes()
        {
            string metadata = $"{FileName}|{ChunkIndex}|{TotalChunks}";
            return Encoding.UTF8.GetBytes($"{metadata}|{Convert.ToBase64String(Bytes)}");
        }

        public override string ToString()
        {
            return $"Chunk number {ChunkIndex}/{TotalChunks} of file {FileName}.";
        }
    }

    public class ChunkFile
    {
        private const int ChunkSize = 10 * 1024 * 1024;

        public byte[] Bytes { get; private set; }
        public string FileName { get; private set; }
        public int FileSize => Bytes.Length;
        public int ChunksCount => (int)GetTotalChunksNumber();

        public ChunkFile(string fileName, byte[] bytes)
        {
            FileName = fileName;
            Bytes = bytes;
        }

        public IEnumerable<Chunk> GetChunks()
        {
            for (int i = 0; i < ChunksCount; i++)
            {
                yield return new Chunk(FileName, i + 1, ChunksCount, GetChunk(i));
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
            while (true)
            {
                try
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        break;
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine($"Waiting for file '{filePath}' to be ready...");
                    await Task.Delay(500);
                }
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
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

            return new ChunkFile(FileName, fileBytes);
        }
    }
}
