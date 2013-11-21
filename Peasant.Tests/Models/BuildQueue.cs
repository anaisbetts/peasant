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
    public class BuildQueueTests
    {
        [Fact]
        public async Task FullBuildIntegrationTest()
        {
            var cache = new TestBlobCache();
            var client = new GitHubClient(new ProductHeaderValue("Peasant"));

            var fixture = new BuildQueue(client, cache);
            using (fixture.Start()) {
                var result = await fixture.Enqueue("https://github.com/paulcbetts/peasant", "306038897ab7b78e95a0117ecabec76506ebb55d", "https://github.com/paulcbetts/peasant/blob/master/script/cibuild.ps1");{}
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
        public void ProcessSingleBuildIntegrationTest()
        {
            var cache = new TestBlobCache();
            var client = new GitHubClient(new ProductHeaderValue("Peasant"));
            var stdout = new Subject<string>();
            var allLines = stdout.CreateCollection();

            var fixture = new BuildQueue(client, cache);
            var result = fixture.ProcessSingleBuild(new BuildQueueItem() {
                BuildId = 1,
                BuildScriptUrl = "https://github.com/paulcbetts/peasant/blob/master/script/cibuild.ps1",
                RepoUrl = "https://github.com/paulcbetts/peasant",
                SHA1 = "306038897ab7b78e95a0117ecabec76506ebb55d",
            }, stdout).Result;

            Assert.Equal(0, result);
            Assert.NotEmpty(allLines);
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
    }
}
