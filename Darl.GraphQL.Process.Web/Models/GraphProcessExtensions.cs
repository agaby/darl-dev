using Darl.Lineage.Bot;
using Darl.Thinkbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Models
{
    public static  class GraphProcessExtensions
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
