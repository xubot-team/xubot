﻿using Discord;
using Discord.Commands;
using SteamKit2.GC.Dota.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xubot.src.Attributes;

namespace xubot.src.Commands
{
    [Group("help"), Alias("?"), Summary("The savior for the lost.")]
    public class Help : ModuleBase
    {
        public readonly int itemsPerPage = 15;

        [Command("get", RunMode = RunMode.Async), Alias("")]
        public async Task _Help(int page = 1)
        {
            await HelpCmd(page);
        }

        [Command("get", RunMode = RunMode.Async), Alias(""), Summary("Lists data for one command.")]
        public async Task HelpCmd(params string[] lookupAsAll)
        {
            string all = "";
            int page, count;
            count = lookupAsAll.Length;

            if (Int32.TryParse(lookupAsAll[count - 1], out page))
                count--;
            else
                page = 1;

            for (int i = 0; i < count; i++)
                all += lookupAsAll[i] + (i == count - 1 ? "" : " ");

            await HelpHandling(all, page, true);
        }

        /*[Command, Summary("Lists data for one command.")]
        public async Task help(string lookup, bool wGroup = false, int index = 1)
        {
            await helpHandling(lookup, index, wGroup);
        }*/

        [Command("list", RunMode = RunMode.Async), Summary("Lists all commands.")]
        public async Task HelpCmd(int page = 1)
        {
            if (page < 1) page = 1;

            List<CommandInfo> commList = Program.xuCommand.Commands.ToList();

            string items = "";

            int limit = System.Math.Min(commList.Count - ((page - 1) * itemsPerPage), itemsPerPage);
            //await ReplyAsync((limit).ToString());

            int index;

            for (int i = 0; i < limit; i++)
            {
                index = i + (itemsPerPage * (page - 1));

                if (index > commList.Count - 1) { break; }

                string parentForm = GetAllGroups(commList[index].Module);

                items += parentForm + commList[index].Name + "\n";
            }

            if (items == "") items = "There's nothing here, I think you went out of bounds.";

            EmbedBuilder embedd = new EmbedBuilder
            {
                Title = "Help",
                Color = Discord.Color.Magenta,
                Description = "Showing page #" + (page).ToString() + " out of " + (System.Math.Ceiling((float)commList.Count / itemsPerPage)).ToString() + " pages.\nShowing a few of the **" + commList.Count.ToString() + "** cmds.",
                ThumbnailUrl = Program.xuClient.CurrentUser.GetAvatarUrl(),

                Footer = new EmbedFooterBuilder
                {
                    Text = "xubot :p",
                    IconUrl = Program.xuClient.CurrentUser.GetAvatarUrl()
                },
                Timestamp = DateTime.UtcNow,
                Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "List",
                                Value = "```\n" + items + "```" ,
                                IsInline = true
                            }
                        }
            };
            await ReplyAsync("", false, embedd.Build());
        }

        [Command("search", RunMode = RunMode.Async), Summary("Searches all commands with a search term. Deep enables searching the aliases as well, but this takes longer.")]
        public async Task Search(string lookup, bool deep, int page = 1)
        {
            using (Util.WorkingBlock wb = new Util.WorkingBlock(Context))
            {
                if (page < 1) page = 1;

                List<CommandInfo> commList = Program.xuCommand.Commands.ToList();
                List<CommandInfo> compatibles = new List<CommandInfo>();

                bool add;
                foreach (CommandInfo cmd in commList)
                {
                    add = false;

                    add |= cmd.Name.Contains(lookup);

                    if (cmd.Aliases != null && deep)
                    {
                        foreach (string alias in cmd.Aliases)
                            add |= alias.Contains(lookup);
                    }

                    if (add) compatibles.Add(cmd);
                }

                int limit = System.Math.Min(commList.Count - ((page - 1) * itemsPerPage), itemsPerPage);
                int index;

                string cmds = "";
                for (int i = 0; i < limit; i++)
                {
                    index = i + (itemsPerPage * (page - 1));

                    if (index > compatibles.Count - 1) { break; }

                    cmds += GetAllGroups(compatibles[index].Module) + compatibles[index].Name + "\n";
                }
                if (cmds == "") cmds = "I don't think any command called that exists...";

                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Help - Search",
                    Color = Discord.Color.Magenta,
                    Description = "Showing page #" + (page).ToString() + " out of " + (System.Math.Ceiling((float)compatibles.Count / itemsPerPage)).ToString() + " pages.\nShowing a few of the **" + compatibles.Count.ToString() + "** cmds with the lookup.",
                    ThumbnailUrl = Program.xuClient.CurrentUser.GetAvatarUrl(),

                    Footer = new EmbedFooterBuilder
                    {
                        Text = "xubot :p",
                        IconUrl = Program.xuClient.CurrentUser.GetAvatarUrl()
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Search Results",
                                Value = "```\n" + cmds + "```" ,
                                IsInline = true
                            }
                        }
                };
                await ReplyAsync("", false, embedd.Build());
            }
        }

        public async Task HelpHandling(string lookup, int index = 1, bool exact = false)
        {
            try
            {
                List<CommandInfo> commList_e = Program.xuCommand.Commands.ToList();
                List<ModuleInfo> moduList_e = Program.xuCommand.Modules.ToList();

                commList_e = commList_e.FindAll(ci => ci.Name == (lookup.Split(' ').Last()));

                List<CommandInfo> commList = commList_e;

                if (exact)
                {
                    commList = new List<CommandInfo>();
                    foreach (var item in commList_e)
                    {
                        foreach (var alias in item.Aliases)
                        {
                            if (alias == lookup)
                            {
                                commList.Add(item);
                                //item.Attributes.Contains(new DeprecatedAttribute());
                            }
                        }
                    }
                }

                int allMatchs = commList.Count;

                CommandInfo comm;

                try
                {
                    comm = commList[index - 1];
                }
                catch (System.ArgumentOutOfRangeException aoore)
                {
                    //not a command
                    GroupHandling(lookup);
                    return;
                }

                string all_alias = "";
                IReadOnlyList<string> _aliases = comm.Aliases ?? new List<string>();

                string trueName = comm.Name;

                if (_aliases.ToList().Find(al => al == lookup) == lookup)
                {
                    trueName = lookup;
                }

                if (_aliases.Count != 0)
                {
                    foreach (string alias in comm.Aliases)
                    {
                        all_alias += alias + "\n";
                    }
                }

                string all_para = "No parameters.";
                IReadOnlyList<ParameterInfo> _params = comm.Parameters.ToList() ?? new List<ParameterInfo>();

                if (_params.Count != 0)
                {
                    all_para = "";
                    foreach (var para in comm.Parameters)
                    {
                        all_para += (para.IsMultiple ? "params " : "") + Util.Str.SimplifyTypes(para.Type.ToString()) + " " + para.Name + (para.IsOptional ? " (optional)" : "") +"\n";
                    }
                }

                string trueSumm = "No summary given.";
                if (comm.Summary != null) trueSumm = comm.Summary;

                bool dep = comm.Attributes.Contains(new DeprecatedAttribute()) || comm.Module.Attributes.Contains(new DeprecatedAttribute());

                string nsfwPossibility = comm.Attributes.Where(x => x is NSFWPossibiltyAttribute).Count() > 0 ? (comm.Attributes.First(x => x is NSFWPossibiltyAttribute) as NSFWPossibiltyAttribute).warnings : "";
                nsfwPossibility += comm.Module.Attributes.Where(x => x is NSFWPossibiltyAttribute).Count() > 0 ? "Groupwide:\n\n" + (comm.Module.Attributes.First(x => x is NSFWPossibiltyAttribute) as NSFWPossibiltyAttribute).warnings : "";

                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Help",
                    Color = Discord.Color.Magenta,
                    Description = "The newer *better* help. Showing result #" + (index).ToString() + " out of " + allMatchs.ToString() + " match(s).",
                    ThumbnailUrl = Program.xuClient.CurrentUser.GetAvatarUrl(),

                    Footer = new EmbedFooterBuilder
                    {
                        Text = "xubot :p",
                        IconUrl = Program.xuClient.CurrentUser.GetAvatarUrl()
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Command Name",
                                Value = "`" + trueName + "`" ,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Summary",
                                Value = trueSumm,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Known Aliases",
                                Value = "```\n" + all_alias + "```",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Parameters",
                                Value = "```cs\n" + all_para + "```",
                                IsInline = true
                            }
                        }
                };

                if (dep) embedd.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = "Deprecated",
                    Value = "__**All commands in this group are deprecated. They are going to be removed in a future update.**__",
                    IsInline = true
                });

                if (nsfwPossibility != "") embedd.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = "NSFW Possibility",
                    Value = "This group has the possibility of showing NSFW content. __*NSFW will* ***NOT*** *be shown that is NSFW unless it is in a channel specified as NSFW.*__\nThe following is stated as a warning for what type of content could be shown:\n**" + nsfwPossibility + "**",
                    IsInline = true
                });

                await ReplyAsync("", false, embedd.Build());
            }
            catch (Exception e)
            {
                await Util.Error.BuildError(e, Context);
            }
        }

        public async Task GroupHandling(string lookup)
        {
            try
            {
                List<ModuleInfo> moduList = Program.xuCommand.Modules.ToList().FindAll(ci => ci.Group == (lookup.Split(' ').Last()));

                int allMatchs = moduList.Count;

                ModuleInfo group;

                try
                {
                    group = moduList[0];
                }
                catch (System.ArgumentOutOfRangeException aoore)
                {
                    //not a command OR group
                    await ReplyAsync("This command does not exist. (Trust me, I looked everywhere.)\nMaybe try the full name?");
                    return;
                }

                string all_alias = "";
                IReadOnlyList<string> _aliases = group.Aliases ?? new List<string>();

                string trueName = group.Name;

                if (_aliases.ToList().Find(al => al == lookup) == lookup)
                {
                    trueName = lookup;
                }

                if (_aliases.Count != 0)
                {
                    foreach (string alias in group.Aliases)
                    {
                        all_alias += alias + "\n";
                    }
                }

                string commands = "None";
                string str;
                if (group.Commands.Count != 0)
                {
                    commands = "";
                    foreach (var cmd in group.Commands)
                    {
                        str = group.Name + " " + cmd.Name + "\n";
                        if (!commands.Contains(str)) commands += str;
                    }
                }

                string subgroup = "None";
                if (group.Submodules.Count != 0)
                {
                    subgroup = "";
                    foreach (var sub in group.Submodules)
                    {
                        subgroup += group.Name + " " + sub.Name + "\n";
                    }
                }

                string trueSumm = "No summary given.";
                if (group.Summary != null) trueSumm = group.Summary;

                bool dep = group.Attributes.Contains(new DeprecatedAttribute());
                string nsfwPossibility = group.Attributes.Contains(new NSFWPossibiltyAttribute()) ? (group.Attributes.First(x => x is NSFWPossibiltyAttribute) as NSFWPossibiltyAttribute).warnings : null;

                EmbedBuilder embedd = new EmbedBuilder
                {
                    Title = "Help",
                    Color = Discord.Color.Magenta,
                    Description = "The newer *better* help. For more specifics, combine the group and command.", //Showing result #" + (index).ToString() + " out of " + allMatchs.ToString() + " match(s).",
                    ThumbnailUrl = Program.xuClient.CurrentUser.GetAvatarUrl(),

                    Footer = new EmbedFooterBuilder
                    {
                        Text = "xubot :p",
                        IconUrl = Program.xuClient.CurrentUser.GetAvatarUrl()
                    },
                    Timestamp = DateTime.UtcNow,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Module Name",
                                Value = "`" + trueName + "`" ,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Group",
                                Value = group.Group,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Summary",
                                Value = trueSumm,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Known Aliases",
                                Value = "```" + all_alias + "```",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Commands in Group",
                                Value = "```" + commands + "```",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Subgroups in Group",
                                Value = "```" + subgroup + "```",
                                IsInline = true
                            }
                        }
                };

                if (dep) embedd.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = "Deprecated",
                    Value = "__**All commands in this group are deprecated. They are going to be removed in a future update.**__",
                    IsInline = true
                });

                if (nsfwPossibility != null) embedd.Fields.Add(new EmbedFieldBuilder()
                {
                    Name = "NSFW Possibility",
                    Value = "This group has the possibility of showing NSFW content. __*NSFW will* ***NOT*** *be shown that is NSFW unless it is in a channel specified as NSFW.*__\nThe following is stated as a warning for what type of content could be shown:\n**" + nsfwPossibility + "**",
                    IsInline = true
                 });

                await ReplyAsync("", false, embedd.Build());
            }
            catch (Exception e)
            {
                await Util.Error.BuildError(e, Context);
            }
        }

        private string GetAllGroups(ModuleInfo module)
        {
            if (module == null) return "";
            if (module.Group == null) return "";

            string output = "";
            ModuleInfo check = module;

            do
            {
                output = check.Group + (check.Group != null ? " " : "") + output;
                check = check.Parent;
            } while (check != null);

            return output;
        }
    }
}
