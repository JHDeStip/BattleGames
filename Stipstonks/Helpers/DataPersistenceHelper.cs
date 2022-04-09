using AutoMapper;
using Stip.Stipstonks.Common;
using Stip.Stipstonks.Models;
using System.IO;
using System.Threading.Tasks;

namespace Stip.Stipstonks.Helpers
{
    public class DataPersistenceHelper : IInjectable
    {
        public EnvironmentHelper EnvironmentHelper { get; set; }
        public FileHelper FileHelper { get; set; }
        public JsonHelper JsonHelper { get; set; }
        public IMapper Mapper { get; set; }
        public ApplicationContext ApplicationContext { get; set; }

        private string DataFilePath
            => Path.Combine(EnvironmentHelper.ExecutableDirectory, "Data.json");

        public async Task<ActionResult> LoadDataAsync()
        {
            var fileStreamResult = FileHelper.OpenStream(
                DataFilePath,
                FileMode.Open);
            if (!fileStreamResult.IsSuccess)
            {
                return ActionResult.Failure;
            }

            var deserializeResult = await JsonHelper.DeserializeFromUtf8StreamAsync<JsonModels.Data>(
                fileStreamResult.Data);
            if (!deserializeResult.IsSuccess)
            {
                return ActionResult.Failure;
            }

            var mapped = Mapper.Map<Data>(deserializeResult.Data);

            ApplicationContext.Config = mapped.Config;
            ApplicationContext.Products = mapped.Products;

            return ActionResult.Success;
        }

        public virtual async Task<ActionResult> SaveDataAsync()
        {
            var fileStreamResult = FileHelper.OpenStream(
                DataFilePath,
                FileMode.Create);
            if (!fileStreamResult.IsSuccess)
            {
                return ActionResult.Failure;
            }
            
            using (var fileStream = fileStreamResult.Data)
            {
                return await JsonHelper.SerializeToUtf8StreamAsync(
                    Mapper.Map<JsonModels.Data>(new Data
                    {
                        Config = ApplicationContext.Config,
                        Products = ApplicationContext.Products
                    }),
                    fileStreamResult.Data);
            }
        }
    }
}
