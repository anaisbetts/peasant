using Akavache;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;

namespace Peasant.Models.Tests
{
    public static class TestBuild
    {
        public const string RepoUrl = "https://github.com/paulcbetts/peasant";
        public const string BuildScriptUrl = "https://github.com/paulcbetts/peasant/blob/master/script/cibuild.ps1";

        public const string PassingBuildSHA1 = "46c20227bb08185215f5b3d9519297142873b261";
        public const string FailingBecauseOfMsbuildSHA1 = "46c20227bb08185215f5b3d9519297142873b261";
    }

    public class BuildQueueTests
    {
        [Fact]
        public async Task FullBuildIntegrationTest()
        {
            var cache = new TestBlobCache();
            var client = new GitHubClient(new ProductHeaderValue("Peasant"));

            var fixture = new BuildQueue(client, cache);
            using (fixture.Start()) {
                var result = await fixture.Enqueue(TestBuild.RepoUrl, TestBuild.PassingBuildSHA1, TestBuild.BuildScriptUrl);
            }
        }

        [Fact]
        public void BuildsThatFailShouldBeRecorded()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildsThatSucceedShouldBeRecorded()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task ProcessSingleBuildIntegrationTest()
        {
            var cache = new TestBlobCache();
            var client = new GitHubClient(new ProductHeaderValue("Peasant"));
            var stdout = new Subject<string>();
            var allLines = stdout.CreateCollection();

            var fixture = new BuildQueue(client, cache);
            var result = await fixture.ProcessSingleBuild(new BuildQueueItem() {
                BuildId = 1,
                BuildScriptUrl = TestBuild.BuildScriptUrl,
                RepoUrl = TestBuild.RepoUrl,
                SHA1 = TestBuild.PassingBuildSHA1,
            }, stdout);

            var output = allLines.Aggregate(new StringBuilder(), (acc, x) => { acc.AppendLine(x); return acc; }).ToString();
            Console.WriteLine(output);

            Assert.Equal(0, result);
            Assert.False(String.IsNullOrWhiteSpace(output));
        }

        [Fact]
        public async Task ProcessSingleBuildThatFails()
        {
            var cache = new TestBlobCache();
            var client = new GitHubClient(new ProductHeaderValue("Peasant"));
            var stdout = new Subject<string>();
            var allLines = stdout.CreateCollection();

            var fixture = new BuildQueue(client, cache);
            var result = default(int);
            bool shouldDie = true;

            try {
                // NB: This build fails because NuGet package restore wasn't set 
                // up properly, so MSBuild is missing a ton of assemblies
                result = await fixture.ProcessSingleBuild(new BuildQueueItem() {
                    BuildId = 1,
                    BuildScriptUrl = TestBuild.BuildScriptUrl,
                    RepoUrl = TestBuild.RepoUrl,
                    SHA1 = TestBuild.FailingBecauseOfMsbuildSHA1,
                }, stdout);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                shouldDie = false;
            }

            var output = allLines.Aggregate(new StringBuilder(), (acc, x) => { acc.AppendLine(x); return acc; }).ToString();
            Console.WriteLine(output);

            Assert.False(shouldDie);
        }

        [Fact]
        public void PausingTheQueueShouldntLoseBuilds()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void FailTheBuildIfBuildScriptUrlIsBogus()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void FailTheBuildIfBuildScriptUrlIsValidBut404s()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildOutputForQueuedBuildsShouldHaveTheBuildId()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildOutputForInProgressBuildsShouldHaveBuildOutput()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildOutputForFinishedBuildsShouldHaveBuildOutput()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildOutputForUnknownBuildsShouldThrow()
        {
            throw new NotImplementedException();
        }
    }
}