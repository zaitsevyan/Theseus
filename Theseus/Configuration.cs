//
//  File: Configuration.cs
//  Created: 29.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using NLog;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Theseus {
    public class Configuration {
        public class Plugin {
            [JsonProperty("class")]
            public string Class = null;

            [JsonProperty("config")]
            public Dictionary<String, Object> Config = new Dictionary<string, object>();
        }
        [JsonProperty("adapters")]
        public List<Plugin> Adapters = new List<Plugin>();

        [JsonProperty("modules")]
        public List<Plugin> Modules = new List<Plugin>();

        private static async Task<String> ReadFile(Logger logger, String fileName, CancellationToken cancellationToken){
            try {
                using (FileStream sourceStream = new FileStream(fileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true)) {
                    byte[] buffer = new byte[sourceStream.Length];
                    int bytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    return Encoding.UTF8.GetString(buffer, 0, bytes);
                }
            }
            catch (IOException e) {
                logger.Error(e);
            }
            return null;
        }

        public static async Task<Configuration> Parse(String fileName, CancellationToken cancellationToken) {
            var logger = LogManager.GetLogger("Configuration");
            string config = await ReadFile(logger, fileName, cancellationToken);
            if (config == null)
                return new Configuration();
            var configuration = JsonConvert.DeserializeObject<Configuration>(config, new JsonConverter[]{new NestedDictionaryConverter()});
            return configuration ?? new Configuration();
        }

        private class NestedDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
        {
            public override IDictionary<string, object> Create(Type objectType)
            {
                return new Dictionary<string, object>();
            }

            public override bool CanConvert(Type objectType)
            {
                // in addition to handling IDictionary<string, object>
                // we want to handle the deserialization of dict value
                // which is of type object
                return objectType == typeof(object) || base.CanConvert(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartObject
                    || reader.TokenType == JsonToken.Null)
                    return base.ReadJson(reader, objectType, existingValue, serializer);

                // if the next token is not an object
                // then fall back on standard deserializer (strings, numbers etc.)
                return serializer.Deserialize(reader);
            }
        }
    }
}

