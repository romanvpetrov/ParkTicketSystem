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
    private byte[]? _stagedContents;

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
            _stagedContents = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        }
    }

    public void Commit()
    {
        lock (_sync)
        {
            if (_stagedContents is null)
            {
                return;
            }

            Write(_stagedContents);
            _stagedContents = null;
        }
    }

    private T Load()
    {
        if (_stagedContents is not null)
        {
            return JsonSerializer.Deserialize<T>(_stagedContents, SerializerOptions)
                ?? throw new InvalidOperationException($"Staged repository data for '{_filePath}' is invalid.");
        }

        if (!File.Exists(_filePath))
        {
            var initialValue = _createInitialValue();
            Write(JsonSerializer.SerializeToUtf8Bytes(initialValue, SerializerOptions));
            return initialValue;
        }

        using var stream = File.OpenRead(_filePath);
        return JsonSerializer.Deserialize<T>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"File '{_filePath}' does not contain valid repository data.");
    }

    private void Write(byte[] contents)
    {
        var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllBytes(temporaryFilePath, contents);
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
