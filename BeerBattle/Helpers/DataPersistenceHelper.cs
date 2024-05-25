﻿using Stip.BattleGames.Common;
using Stip.BattleGames.Common.Helpers;
using Stip.BeerBattle.JsonModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stip.BeerBattle.Helpers;

public class DataPersistenceHelper(
    EnvironmentHelper _environmentHelper,
    FileHelper _fileHelper,
    JsonHelper _jsonHelper,
    ApplicationContext _applicationContext)
    : IInjectable
{
    private string DataFilePath
        => Path.Combine(_environmentHelper.ExecutableDirectory, "Data.json");

    public async Task<ActionResult> LoadDataAsync()
    {
        var fileStreamResult = _fileHelper.OpenStream(
            DataFilePath,
            FileMode.Open);
        if (!fileStreamResult.IsSuccess)
        {
            return ActionResult.Failure;
        }

        var deserializeResult = await _jsonHelper.DeserializeFromUtf8StreamAsync<Data>(
            fileStreamResult.Data);
        if (!deserializeResult.IsSuccess)
        {
            return ActionResult.Failure;
        }

        _applicationContext.Config = deserializeResult.Data.ToConfig();
        _applicationContext.Groups = deserializeResult.Data.Groups.Select(x => x.ToModel()).ToList();
        _applicationContext.Products = deserializeResult.Data.Products.Select(x => x.ToModel()).ToList();

        return ActionResult.Success;
    }

    public virtual async Task<ActionResult> SaveDataAsync()
    {
        var fileStreamResult = _fileHelper.OpenStream(
            DataFilePath,
            FileMode.Create);
        if (!fileStreamResult.IsSuccess)
        {
            return ActionResult.Failure;
        }

        using var fileStream = fileStreamResult.Data;

        return await _jsonHelper.SerializeToUtf8StreamAsync(
            Data.From(
                _applicationContext.Config,
                _applicationContext.Groups,
                _applicationContext.Products),
            fileStreamResult.Data);
    }
}
