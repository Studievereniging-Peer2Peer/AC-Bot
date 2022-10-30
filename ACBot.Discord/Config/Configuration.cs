using Newtonsoft.Json;

namespace ACBot.Discord.Config;

public class Configuration
{
    private const string ConfigurationStorageLocation = "configuration.json";

    public Dictionary<string, object?> ConfigurationMappings { get; set; }
        = new();

    public object? this[string key]
        => ConfigurationMappings[key];

    public async Task Save()
        => await File.WriteAllTextAsync(
            ConfigurationStorageLocation,
            JsonConvert.SerializeObject(ConfigurationMappings)
        );

    public async Task Load()
    {
        if (!File.Exists(ConfigurationStorageLocation))
            return;

        ConfigurationMappings = JsonConvert.DeserializeObject<Dictionary<string, object?>>(
            await File.ReadAllTextAsync(ConfigurationStorageLocation)
        ) ?? new Dictionary<string, object?>();
    }
}