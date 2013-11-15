using Akavache;
using Octokit;
using Punchclock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Peasant.Models
{
    public class QueueItem 
    {
        public long BuildId { get; set; }
        public string RepoUrl { get; set; }
        public string SHA1 { get; set; }
        public string BuildScriptUrl { get; set; }
        public string BuildOutput { get; set; }
    }

    public class BuildQueue
    {
        readonly OperationQueue opQueue = new OperationQueue(2);
        readonly IBlobCache blobCache;
        readonly GitHubClient client;
        readonly Subject<QueueItem> enqueueSubject = new Subject<QueueItem>();
        readonly Subject<QueueItem> finishedBuilds = new Subject<QueueItem>();
        readonly Func<QueueItem, Task<string>> processBuildFunc;

        long nextBuildId;

        public BuildQueue(IBlobCache cache, GitHubClient githubClient, Func<QueueItem, string> processBuildFunc = null)
        {
            blobCache = cache;
            client = githubClient;
            this.processBuildFunc = processBuildFunc;
        }

        public IObservable<QueueItem> Enqueue(string repoUrl, string sha1, string buildScriptUrl)
        {
            var buildId = Interlocked.Increment(ref nextBuildId);

            enqueueSubject.OnNext(new QueueItem() {
                BuildId = buildId,
                RepoUrl = repoUrl,
                SHA1 = sha1,
                BuildScriptUrl = buildScriptUrl,
            });

            return finishedBuilds.Where(x => x.BuildId == buildId).Take(1);
        }

        public IDisposable Start()
        {
            var enqueueWithSave = enqueueSubject
                .SelectMany(x => blobCache.InsertObject("build_" + x.BuildId, x).Select(_ => x));

            var ret = blobCache.GetAllObjects<QueueItem>()
                .Do(x => nextBuildId = x.Max(y => y.BuildId) + 1)
                .SelectMany(x => x.ToObservable())
                .Concat(enqueueWithSave)
                .SelectMany(x => opQueue.Enqueue(10, () => processBuildFunc(x))
                    .ToObservable()
                    .Select(y => new { Build = x, Output = y }))
                .SelectMany(async x => {
                    await blobCache.Invalidate("build_" + x.Build.BuildId);
                    await blobCache.Insert("buildoutput_" + x.Build.BuildId, Encoding.UTF8.GetBytes(x.Output));

                    x.Build.BuildOutput = x.Output;
                    return x.Build;
                })
                .Multicast(finishedBuilds);

            return ret.Connect();
        }

    }
}