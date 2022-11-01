using Newtonsoft.Json;

namespace ACBot.Discord.Config;

public class Configuration
{
    /// <summary>
    /// The location at which the configuration file is stored.
    /// </summary>
    private const string ConfigurationStorageLocation = "configuration.json";

    /// <summary>
    /// The configuration mappings of the discord bot.
    /// </summary>
    public Dictionary<string, IConfigurationModel> ConfigurationMappings { get; set; }
        = new();

    public object? this[string key]
        => ConfigurationMappings[key];

    /// <summary>
    /// Store the configuration mappings in a json file for persistency.
    /// Takes into account types when dealing with abstract/interface classes.
    /// </summary>
    public async Task Save()
        => await File.WriteAllTextAsync(
            ConfigurationStorageLocation,
            JsonConvert.SerializeObject(
                ConfigurationMappings,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }
            )
        );

    /// <summary>
    /// Load the configuration mappings in a json file for persistency.
    /// Takes into account types when dealing with abstract/interface classes.
    /// </summary>
    public async Task Load()
    {
        if (!File.Exists(ConfigurationStorageLocation))
            return;

        ConfigurationMappings = JsonConvert.DeserializeObject<Dictionary<string, IConfigurationModel>>(
            await File.ReadAllTextAsync(ConfigurationStorageLocation),
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            }
        ) ?? new Dictionary<string, IConfigurationModel>();
    }
}