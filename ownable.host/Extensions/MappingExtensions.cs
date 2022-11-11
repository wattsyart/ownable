using System.Text.Json;

namespace ownable.host.Extensions
{
    public static class MappingExtensions
    {
        public static void MapFrom(this JsonSerializerOptions to, JsonSerializerOptions from)
        {
            to.Converters.Clear();
            foreach (var converter in from.Converters)
                to.Converters.Add(converter);

            to.AllowTrailingCommas = from.AllowTrailingCommas;
            to.DefaultBufferSize = from.DefaultBufferSize;
            to.DefaultIgnoreCondition = from.DefaultIgnoreCondition;
            to.DictionaryKeyPolicy = from.DictionaryKeyPolicy;
            to.Encoder = from.Encoder;
            to.IgnoreReadOnlyFields = from.IgnoreReadOnlyFields;
            to.IgnoreReadOnlyProperties = from.IgnoreReadOnlyProperties;
            to.IncludeFields = from.IncludeFields;
            to.NumberHandling = from.NumberHandling;
            to.PropertyNameCaseInsensitive = from.PropertyNameCaseInsensitive;
            to.PropertyNamingPolicy = from.PropertyNamingPolicy;
            to.ReadCommentHandling = from.ReadCommentHandling;
            to.ReferenceHandler = from.ReferenceHandler;
            to.UnknownTypeHandling = from.UnknownTypeHandling;
            to.WriteIndented = from.WriteIndented;
            to.MaxDepth = from.MaxDepth;
        }
    }
}
