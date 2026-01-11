using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CodingMCP.Configuration;

public interface IConfigurationLoader
{
    CodingSettings LoadConfiguration();
    void SaveConfiguration(CodingSettings settings);
    void ReloadConfiguration();
}

public class ConfigurationLoader : IConfigurationLoader
{
    private readonly string _configPath;
    private readonly ILogger<ConfigurationLoader>? _logger;
    private CodingSettings? _cachedSettings;
    private DateTime _lastLoadTime;

    public ConfigurationLoader(string? configPath = null, ILogger<ConfigurationLoader>? logger = null)
    {
        _configPath = configPath ?? System.IO.Path.Combine(AppContext.BaseDirectory, "config.json");
        _logger = logger;
        _lastLoadTime = DateTime.MinValue;
    }

    public CodingSettings LoadConfiguration()
    {
        // Return cached settings if loaded recently (within 5 seconds)
        if (_cachedSettings != null && (DateTime.UtcNow - _lastLoadTime).TotalSeconds < 5)
        {
            return _cachedSettings;
        }

        try
        {
            if (!File.Exists(_configPath))
            {
                _logger?.LogWarning("Configuration file not found at {Path}, creating default", _configPath);
                var defaultSettings = new CodingSettings();
                SaveConfiguration(defaultSettings);
                return defaultSettings;
            }

            var json = File.ReadAllText(_configPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var settings = JsonSerializer.Deserialize<CodingSettings>(json, options);
            
            if (settings == null)
            {
                _logger?.LogError("Failed to deserialize configuration, using defaults");
                return new CodingSettings();
            }

            _cachedSettings = settings;
            _lastLoadTime = DateTime.UtcNow;
            
            _logger?.LogInformation("Configuration loaded from {Path}", _configPath);
            return settings;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading configuration from {Path}, using defaults", _configPath);
            return new CodingSettings();
        }
    }

    public void SaveConfiguration(CodingSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_configPath, json);
            
            _cachedSettings = settings;
            _lastLoadTime = DateTime.UtcNow;
            
            _logger?.LogInformation("Configuration saved to {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving configuration to {Path}", _configPath);
            throw;
        }
    }

    public void ReloadConfiguration()
    {
        _cachedSettings = null;
        _lastLoadTime = DateTime.MinValue;
        _logger?.LogInformation("Configuration cache cleared, will reload on next access");
    }
}
