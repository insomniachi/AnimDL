﻿using System;
using System.Collections.Generic;
using System.IO;
using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Core.Contracts.Services;
using AnimDL.WinUI.Core.Helpers;
using AnimDL.WinUI.Models;
using Microsoft.Extensions.Options;

namespace AnimDL.WinUI.Services;

public class LocalSettingsServiceUnpackaged : ILocalSettingsService
{
    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private IDictionary<string, object> _settings;

    public LocalSettingsServiceUnpackaged(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _options = options.Value;

        var folderPath = Path.Combine(_localAppData, _options.ApplicationDataFolder);
        var fileName = _options.LocalSettingsFile;
        _settings = _fileService.Read<IDictionary<string, object>>(folderPath, fileName) ?? new Dictionary<string, object>();
    }

    public T ReadSetting<T>(string key)
    {
        if (_settings.TryGetValue(key, out object obj))
        {
            return Json.ToObject<T>((string)obj);
        }

        return default;
    }

    public void SaveSetting<T>(string key, T value)
    {
        _settings[key] = Json.Stringify(value);
        var folderPath = Path.Combine(_localAppData, _options.ApplicationDataFolder);
        var fileName = _options.LocalSettingsFile;
        _fileService.Save(folderPath, fileName, _settings);
    }
}