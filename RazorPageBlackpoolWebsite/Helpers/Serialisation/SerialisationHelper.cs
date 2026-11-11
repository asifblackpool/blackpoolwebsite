using Content.Modelling.Models.Canvas.Lists;
using Content.Modelling.Models.GenericTypes;
using Newtonsoft.Json;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;

namespace RazorPageBlackpoolWebsite.Helpers.Serialisation
{
    public class SerializationHelper : ISerializationHelper
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Converters = new List<JsonConverter>
            {
                new ValueConverter(),
                new FragmentConverter()
            },
            NullValueHandling = NullValueHandling.Ignore
        };

        public T? Deserialize<T>(SerialisedItem item)
        {
            return JsonConvert.DeserializeObject<T>(item.Content, Settings);
        }

        public Task<T?> DeserializeAsync<T>(SerialisedItem item)
        {
            // JsonConvert.DeserializeObject is synchronous; wrap it so the
            // signature stays async without spinning up a thread pool thread
            // for no reason.
            try
            {
                var result = JsonConvert.DeserializeObject<T>(item.Content, Settings);
                return Task.FromResult(result);
            }
            catch
            {
                return Task.FromResult<T?>(default);
            }
        }
    }

}