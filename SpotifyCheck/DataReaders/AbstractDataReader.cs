using Microsoft.Extensions.Logging;

namespace SpotifyCheck.DataReaders;

public abstract class AbstractDataReader<T>
{
    protected ILogger<AbstractDataReader<T>> Logger;

    protected AbstractDataReader(ILogger<AbstractDataReader<T>> logger)
    {
        Logger = logger;
    }

    public async IAsyncEnumerable<T> Read(string storage)
    {
        if (!Directory.Exists(storage))
        {
            Logger.LogCritical("Storage not exists {storage}", storage);
            throw new Exception("Storage not exists");
        }

        Logger.LogTrace("Get files in storage {storage}", storage);
        string[] filePaths = Directory.GetFiles(storage, "*.txt");

        foreach (string path in filePaths)
        {
            Logger.LogTrace("Read file {filePaths}", filePaths);

            using (StreamReader reader = new(path))
            {
                while (await reader.ReadLineAsync() is { } line)
                {
                    Logger.LogTrace("Read line {line}", line);
                    (bool, T) data = Process(line)!;

                    if (data.Item1)
                        yield return data.Item2;
                }
            }
        }
    }

    protected abstract (bool, T) Process(string text);
}