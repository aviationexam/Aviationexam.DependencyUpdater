using Corvus.Json;
using Corvus.Json.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Aviationexam.DependencyUpdater.ConfigurationParser;

[JsonSchemaTypeGenerator("dependabot-2.0.json")]
[SuppressMessage("Design", "MA0077:A class that provides Equals(T) should implement IEquatable<T>")]
public readonly partial struct DependabotConfiguration
{
    public readonly partial struct Update
    {
        public static ReadOnlySpan<byte> TargetFrameworkUtf8 => "targetFramework"u8;

        public TargetFrameworkEntity? TargetFramework
        {
            get
            {
                if (backing.HasFlag(Backing.JsonElement))
                {
                    if (jsonElementBacking.ValueKind != JsonValueKind.Object)
                    {
                        return null;
                    }

                    if (
                        jsonElementBacking.TryGetProperty(TargetFrameworkUtf8, out var result)
                        && result.GetString() is { } targetFramework
                    )
                    {
                        return new TargetFrameworkEntity(targetFramework);
                    }
                }

                if (backing.HasFlag(Backing.Object))
                {
                    if (objectBacking.TryGetValue(TargetFrameworkUtf8, out var result))
                    {
                        if (
                            result.ValueKind is JsonValueKind.String
                            && result.AsString.GetString() is { } targetFramework
                        )
                        {
                            return new TargetFrameworkEntity(targetFramework);
                        }
                    }
                }

                return null;
            }
        }
    }
}
