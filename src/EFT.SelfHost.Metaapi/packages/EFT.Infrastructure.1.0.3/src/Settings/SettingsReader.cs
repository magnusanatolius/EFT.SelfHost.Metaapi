﻿using Newtonsoft.Json;
using System;
using System.IO;
namespace EFT.Infrastructure.Settings
{
    public class SettingsReader : ISettingsReader
    {
        private readonly string _configurationFilePath;
        private readonly string _sectionNameSuffix;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new SettingsReaderContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public SettingsReader(string configurationFilePath, string sectionNameSuffix = "Settings")
        {
            _configurationFilePath = configurationFilePath;
            _sectionNameSuffix = sectionNameSuffix;
        }
        public T Load<T>() where T : class, new() => Load(typeof(T)) as T;
        public T LoadSection<T>() where T : class, new() => LoadSection(typeof(T)) as T;

        public object Load(Type type)
        {
            if (!File.Exists(_configurationFilePath))
                return Activator.CreateInstance(type);
            var jsonFile = File.ReadAllText(_configurationFilePath);
            return JsonConvert.DeserializeObject(jsonFile, type, JsonSerializerSettings);
        }
        public object LoadSection(Type type)
        {
            if (!File.Exists(_configurationFilePath))
                return Activator.CreateInstance(type);

            var jsonFile = File.ReadAllText(_configurationFilePath);
            var section = ToCamelCase(type.Name.Replace(_sectionNameSuffix, string.Empty));
            var settingsData = JsonConvert.DeserializeObject<dynamic>(jsonFile, JsonSerializerSettings);
            var settingsSection = settingsData[section];

            return settingsSection == null
           ? Activator.CreateInstance(type)
           : JsonConvert.DeserializeObject(JsonConvert.SerializeObject(settingsSection), type,
               JsonSerializerSettings);
        }

        private static string ToCamelCase(string text)
            => String.IsNullOrWhiteSpace(text) ? string.Empty
            : $"{text[0].ToString().ToLowerInvariant()}{text.Substring(1)}";

    }
}

