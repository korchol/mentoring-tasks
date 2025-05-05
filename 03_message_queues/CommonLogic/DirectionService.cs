namespace CommonLogic
{
    public class DirectionService
    {
        public static string SetupFolder(string folderName)
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var folder = FindDirectory(appDirectory, "03_message_queues") + $"\\{folderName}";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Console.WriteLine($"Input folder '{folder}' created.");
            }

            return folder;
        }

        private static string FindDirectory(string startPath, string targetFolderName)
        {
            var directory = new DirectoryInfo(startPath);

            while (directory != null)
            {
                if (directory.Name == targetFolderName)
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            return string.Empty;
        }
    }
}
