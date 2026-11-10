using System.Text.Json.Serialization;

namespace AudioSwitch.Core.Settings;

// Discover the settings graph at build time instead of reflecting over it at startup.
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class SettingsJsonContext : JsonSerializerContext;
