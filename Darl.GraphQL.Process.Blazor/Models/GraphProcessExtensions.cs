using Darl.Lineage.Bot;
using Darl.Thinkbase;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Darl.GraphQL.Process.Blazor.Models
{
    public static class GraphProcessExtensions
    {

        public static IObservable<KnowledgeState> Process(this IObservable<KnowledgeState> ks, ISubject<KnowledgeState> _knowledgeStateStream, IBotProcessing bot, GraphProcess process, string? target)
        {
            ks.Subscribe(async x =>
            {
                var kresp = await bot.Seek(x, target, new List<string>(), "adjective:5500");
                _knowledgeStateStream.OnNext(kresp);
            });
            return _knowledgeStateStream.AsObservable();
        }
    }
}
