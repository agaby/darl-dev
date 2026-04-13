/// </summary>

using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;

namespace Darl.GraphQL.Container.Ui.Playground.Internal
{

    // https://docs.microsoft.com/en-us/aspnet/core/mvc/razor-pages/?tabs=netcore-cli
    public class PlaygroundPageModel
    {

        private string? playgroundCSHtml;

        private readonly GraphQLPlaygroundOptions options;

        public PlaygroundPageModel(GraphQLPlaygroundOptions options)
        {
            this.options = options;
        }

        public string Render()
        {
            if (playgroundCSHtml != null)
            {
                return playgroundCSHtml;
            }
            var assembly = typeof(PlaygroundPageModel).GetTypeInfo().Assembly;
            using (var manifestResourceStream = assembly.GetManifestResourceStream("Darl.GraphQL.Ui.Internal.playground.cshtml"))
            {
                if (manifestResourceStream != null)
                {
                    using (var streamReader = new StreamReader(manifestResourceStream))
                    {
                        var builder = new StringBuilder(streamReader.ReadToEnd());
                        builder.Replace("@Model.GraphQLEndPoint",
                            options.GraphQLEndPoint);
                        builder.Replace("@Model.GraphQLConfig",
                            JsonConvert.SerializeObject(options.GraphQLConfig));
                        builder.Replace("@Model.PlaygroundSettings",
                            JsonConvert.SerializeObject(options.PlaygroundSettings));
                        playgroundCSHtml = builder.ToString();
                        return this.Render();
                    }
                }
            }
            return string.Empty;
        }

    }

}
