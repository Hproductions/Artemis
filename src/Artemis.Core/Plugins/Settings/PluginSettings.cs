﻿using System.Collections.Generic;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core
{
    /// <summary>
    ///     <para>This contains all the settings for your plugin. To access a setting use <see cref="GetSetting{T}" />.</para>
    ///     <para>To use this class, inject it into the constructor of your plugin.</para>
    /// </summary>
    public class PluginSettings
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly Dictionary<string, IPluginSetting> _settingEntities;

        internal PluginSettings(Plugin plugin, IPluginRepository pluginRepository)
        {
            Plugin = plugin;
            Plugin.Settings = this;

            _pluginRepository = pluginRepository;
            _settingEntities = new Dictionary<string, IPluginSetting>();
        }

        /// <summary>
        ///     Gets the plugin these settings belong to
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the setting with the provided name. If the setting does not exist yet, it is created.
        /// </summary>
        /// <typeparam name="T">The type of the setting, can be any serializable type</typeparam>
        /// <param name="name">The name of the setting</param>
        /// <param name="defaultValue">The default value to use if the setting does not exist yet</param>
        /// <returns></returns>
        public PluginSetting<T> GetSetting<T>(string name, T? defaultValue = default)
        {
            lock (_settingEntities)
            {
                // Return cached value if available
                if (_settingEntities.ContainsKey(name))
                    return (PluginSetting<T>) _settingEntities[name];
                // Try to find in database
                PluginSettingEntity settingEntity = _pluginRepository.GetSettingByNameAndGuid(name, Plugin.Guid);
                // If not found, create a new one
                if (settingEntity == null)
                {
                    settingEntity = new PluginSettingEntity
                    {
                        Name = name,
                        PluginGuid = Plugin.Guid,
                        Value = CoreJson.SerializeObject(defaultValue)
                    };
                    _pluginRepository.AddSetting(settingEntity);
                }

                PluginSetting<T> pluginSetting = new(_pluginRepository, settingEntity);

                // This overrides null with the default value, I'm not sure if that's desirable because you
                // might expect something to go null and you might not
                // if (pluginSetting.Value == null && defaultValue != null)
                //    pluginSetting.Value = defaultValue;

                _settingEntities.Add(name, pluginSetting);
                return pluginSetting;
            }
        }

        /// <summary>
        ///     Saves all currently loaded settings
        /// </summary>
        public void SaveAllSettings()
        {
            foreach (var (_, pluginSetting) in _settingEntities)
                pluginSetting.Save();
        }

        internal void ClearSettings()
        {
            _settingEntities.Clear();
        }
    }
}