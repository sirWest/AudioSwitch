namespace AudioSwitch.Core.Audio;

public sealed record AudioChange(string? Id, Direction? Direction = null, AudioRole? Role = null);
