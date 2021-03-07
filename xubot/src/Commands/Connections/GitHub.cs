using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GitHub;

namespace xubot.src.Commands.Connections
{
    public class GitHub : ModuleBase
    {
        public static GitHub xuGit = new GitHub();
        public static GitHubClient xuGitClient = new GitHubClient(new OAuth2Token(Program.JSONKeys["keys"].Contents.github.ToString()));
        public static GitRepository xuRepo;
        public static GitRepository xuRepo_Comm;
        public static GitRef[] xuRepo_Refs;
        public static GitRef xuRepo_Ref;
        public static GitCommit xuCommit_Comm;
        public static GitCommit[] xuCommitArr_Comm;

        public async void GetGitHubUpdatesAsync()
        {
            xuRepo = await xuGitClient.GetRepositoryAsync("xubot-team", "xubot");
        }

        [Group("github"), Summary("The wrapper for the Github API.")]
        public class gitCommands : ModuleBase
        {
            [Command("repo"), Summary("Returns information about a GitHub repo.")]
            public async Task RepoInfo(string user, string repo)
            {
                xuRepo_Comm = await xuGitClient.GetRepositoryAsync(user, repo);
                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Information about: " + user + "/" + repo,
                    Color = Discord.Color.Red,
                    Description = "GitHub repo details",
                    ThumbnailUrl = Context.Guild.IconUrl,

                    Footer = new EmbedFooterBuilder
                    {
                        Text = Util.Globals.EmbedFooter
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Created (UTC)",
                                Value = "**" + xuRepo_Comm.CreatedAt.Value.GetDateTime().ToUniversalTime() + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Is Fork?",
                                Value = "**" + xuRepo_Comm.Fork + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Url",
                                Value = xuRepo_Comm.HtmlUrl + "\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Latest Push (UTC)",
                                Value = "**" + xuRepo_Comm.PushedAt.Value.GetDateTime().ToUniversalTime() + "**\n",
                                IsInline = false
                            }
                        }
                };

                await ReplyAsync("", false, embedd.Build());
            }

            [Command("commit"), Summary("Returns information about a GitHub commit based on its SHA.")]
            public async Task CommitInfo(string user, string repo, string sha)
            {
                xuCommit_Comm = await xuGitClient.GetCommitAsync(user, repo, sha);
                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Information about: " + user + "/" + repo + " commit:" + sha,
                    Color = Discord.Color.Red,
                    Description = "GitHub commit details",
                    ThumbnailUrl = Context.Guild.IconUrl,

                    Footer = new EmbedFooterBuilder
                    {
                        Text = Util.Globals.EmbedFooter
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Author Name",
                                Value = "**" + xuCommit_Comm.Author.Name + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Commited by",
                                Value = "**" + xuCommit_Comm.Committer.Name + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Message",
                                Value = xuCommit_Comm.Message + "\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Tree Url",
                                Value = "**" + xuCommit_Comm.Tree.Url + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "SHA",
                                Value = "**" + xuCommit_Comm.Sha + "**\n",
                                IsInline = false
                            }
                        }
                };

                await ReplyAsync("", false, embedd.Build());
            }

            [Command("repo-latest-commit"), Alias("rlc"), Summary("Returns the latest commit on a GitHub repo.")]
            public async Task RepoCommInfo(string user, string repo)
            {
                xuCommitArr_Comm = await xuGitClient.GetRepositoryCommitsAsync(user, repo);
                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Information about: " + user + "/" + repo,
                    Color = Discord.Color.Red,
                    Description = "GitHub repo latest commit details",
                    ThumbnailUrl = Context.Guild.IconUrl,

                    Footer = new EmbedFooterBuilder
                    {
                        Text = Util.Globals.EmbedFooter
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Latest Commit SHA",
                                Value = xuCommitArr_Comm.First<GitCommit>().Sha + "\n",
                                IsInline = false
                            }
                        }
                };

                await ReplyAsync("", false, embedd.Build());
            }

            [Command("latest-ref"), Summary("Returns information on a GitHub repo's latest ref.")]
            public async Task RefsInfo(string user, string repo)
            {
                xuRepo_Refs = await xuGitClient.GetRefsAsync(user, repo);
                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Information about: " + user + "/" + repo,
                    Color = Discord.Color.Red,
                    Description = "GitHub refs details",
                    ThumbnailUrl = Context.Guild.IconUrl,

                    Footer = new EmbedFooterBuilder
                    {
                        Text = Util.Globals.EmbedFooter
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Latest Ref Url",
                                Value = "**" + xuRepo_Refs.Last().Ref + "**\n",
                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Latest Ref SHA",
                                Value = xuRepo_Refs.Last().Object.Sha + "\n",
                                IsInline = false
                            }
                        }
                };

                await ReplyAsync("", false, embedd.Build());
            }
        }
    }
}