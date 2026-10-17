using System.Text.Json;

namespace TicketSystem.Infrastructure.Persistence;

internal sealed class JsonFileStore<T>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly Func<T> _createInitialValue;
    private readonly object _sync = new();

    public JsonFileStore(string dataDirectory, string fileName, Func<T> createInitialValue)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            throw new ArgumentException("Data directory is required.", nameof(dataDirectory));
        }

        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, fileName);
        _createInitialValue = createInitialValue;
    }

    public TResult Read<TResult>(Func<T, TResult> read)
    {
        lock (_sync)
        {
            return read(Load());
        }
    }

    public void Update(Action<T> update)
    {
        lock (_sync)
        {
            var value = Load();
            update(value);
            Save(value);
        }
    }

    private T Load()
    {
        if (!File.Exists(_filePath))
        {
            var initialValue = _createInitialValue();
            Save(initialValue);
            return initialValue;
        }

        using var stream = File.OpenRead(_filePath);
        return JsonSerializer.Deserialize<T>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"File '{_filePath}' does not contain valid repository data.");
    }

    private void Save(T value)
    {
        var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            using (var stream = File.Create(temporaryFilePath))
            {
                JsonSerializer.Serialize(stream, value, SerializerOptions);
            }

            File.Move(temporaryFilePath, _filePath, true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }
}
