using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peasant.Models
{
    public class GitUser
    {
        public string email { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }

    public class Commit
    {
        public List<object> added { get; set; }
        public GitUser author { get; set; }
        public GitUser committer { get; set; }
        public bool distinct { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public List<object> modified { get; set; }
        public List<object> removed { get; set; }
        public string timestamp { get; set; }
        public string url { get; set; }
    }

    public class User
    {
        public string email { get; set; }
        public string name { get; set; }
    }

    public class GitHubRepository
    {
        public int created_at { get; set; }
        public string description { get; set; }
        public bool fork { get; set; }
        public int forks { get; set; }
        public bool has_downloads { get; set; }
        public bool has_issues { get; set; }
        public bool has_wiki { get; set; }
        public string homepage { get; set; }
        public int id { get; set; }
        public string language { get; set; }
        public string master_branch { get; set; }
        public string name { get; set; }
        public int open_issues { get; set; }
        public User owner { get; set; }
        public bool @private { get; set; }
        public int pushed_at { get; set; }
        public int size { get; set; }
        public int stargazers { get; set; }
        public string url { get; set; }
        public int watchers { get; set; }
    }

    public class PostRecieve
    {
        public string after { get; set; }
        public string before { get; set; }
        public List<Commit> commits { get; set; }
        public string compare { get; set; }
        public bool created { get; set; }
        public bool deleted { get; set; }
        public bool forced { get; set; }
        public Commit head_commit { get; set; }
        public User pusher { get; set; }
        public string @ref { get; set; }
        public GitHubRepository repository { get; set; }
    }
}
