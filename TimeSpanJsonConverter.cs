using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBot
{
    /// <summary>
    /// Converts a <see cref="TimeSpan"/> to its string representation.
    /// </summary>
    internal class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}