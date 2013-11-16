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
    public class BuildQueueItem 
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
        readonly Subject<BuildQueueItem> enqueueSubject = new Subject<BuildQueueItem>();
        readonly Subject<BuildQueueItem> finishedBuilds = new Subject<BuildQueueItem>();
        readonly Func<BuildQueueItem, Task<string>> processBuildFunc;

        long nextBuildId;

        public BuildQueue(IBlobCache cache, GitHubClient githubClient, Func<BuildQueueItem, Task<string>> processBuildFunc = null)
        {
            blobCache = cache;
            client = githubClient;
            this.processBuildFunc = processBuildFunc ?? ProcessSingleBuild;
        }

        public IObservable<BuildQueueItem> Enqueue(string repoUrl, string sha1, string buildScriptUrl)
        {
            var buildId = Interlocked.Increment(ref nextBuildId);

            enqueueSubject.OnNext(new BuildQueueItem() {
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

            var ret = blobCache.GetAllObjects<BuildQueueItem>()
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

        public async Task<string> ProcessSingleBuild(BuildQueueItem queueItem)
        {
            var orgs = await client.Organization.GetAllForCurrent();

            var target = Directory.CreateDirectory(Path.GetTempPath() + "\\" + Guid.NewGuid().ToString());

            await Task.Run(() => {
                var creds = new LibGit2Sharp.Credentials() { Username = client.Credentials.Login, Password = client.Credentials.Password };
                LibGit2Sharp.Repository.Clone(queueItem.RepoUrl, target.FullName, credentials: creds);
            });

            throw new NotImplementedException();
        }

        async Task<string> validateBuildUrl(string buildUrl)
        {
            var m = Regex.Match(buildUrl.ToLowerInvariant(), @"https://github.com/(\w+)/(\w+)");
            if (!m.Success) {
                goto fail;
            }

            var org = m.Captures[1].Value;
            var repo = m.Captures[2].Value;

            // Anything from your own repo is :cool:
            if (org == client.Credentials.Login) {
                return null;
            }

            var repoInfo = default(Repository);
            try {
                // XXX: This needs to be a more thorough check, this means any
                // public repo can be used.
                repoInfo = await client.Repository.Get(org, repo);
            } catch (Exception ex) {
                goto fail;
            }

            if (repoInfo != null) return null;

        fail:
            return "Build URL must be hosted on a repo or organization you are a member of and that you have made at least one commit to.";
        }
    }
}