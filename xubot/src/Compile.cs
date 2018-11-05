﻿using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jurassic;
using NLua;
using Discord;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace xubot
{
    public partial class Compile : ModuleBase
    {
        public static ScriptEngine jsEngine = new ScriptEngine();

        //public static string _eval = "";
        //public static string _result = "";
        public static string _result_input = "";

        /// @param engine interp engine
        /// @param description erm...
        /// @param highlight_js_lang lang code for hilighting
        public static Embed BuildEmbed(string lang, string description, string highlight_js_lang, string _eval, string _result)
        {
            EmbedBuilder embedd = new EmbedBuilder
            {
                Title = "**Language:** `" + lang + "`",
                Color = Discord.Color.Orange,
                Description = description,

                Footer = new EmbedFooterBuilder
                {
                    Text = "xubot :p"
                },
                Timestamp = DateTime.UtcNow,
                Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Input",

                                Value = "```" + highlight_js_lang + "\n" + _eval + "```",

                                IsInline = false
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Result",
                                Value = "```\n" + _result + "```",
                                IsInline = false
                            }
                        }
            };

            return embedd.Build();
            //await ReplyAsync("", false, embedd);
        }

        private static string TableToString(LuaTable t)
        {
            object[] keys = new object[t.Keys.Count];
            object[] values = new object[t.Values.Count];
            t.Keys.CopyTo(keys, 0);
            t.Values.CopyTo(values, 0);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < keys.Count(); i++)
            {
                builder.AppendLine($"{keys[i]} = {values[i]} ");
            }

            return builder.ToString();
        }

        [Group("interp")]
        public partial class codeCompile : ModuleBase
        {
            [Command("js", RunMode = RunMode.Async), Summary("Executes JavaScript.")]
            public async Task js(string eval)
            {
                string local_result;
                int _timeout = 15;

                Process code_handler = Process.Start(Environment.CurrentDirectory + "\\code-handler\\xubot-code-compiler.exe", "js " + eval);

                string uri = Path.GetTempPath() + "InterpResult.xubot";

                code_handler.WaitForExit(_timeout * 1000);

                if (!code_handler.HasExited)
                {
                    code_handler.Kill();
                    local_result = _timeout + " seconds past w/o result.";
                    await ReplyAsync("", false, BuildEmbed("Javascript", "using Jurassic", "js", eval, local_result));
                }
                else
                {
                    if (File.Exists(uri))
                    {
                        local_result = File.ReadAllText(uri);
                        File.Delete(uri);

                        await ReplyAsync("", false, BuildEmbed("Javascript", "using Jurassic", "js", eval, local_result));
                    }
                    else
                    {
                        local_result = "Result was not stored.";
                        await ReplyAsync("", false, BuildEmbed("Javascript", "using Jurassic", "js", eval, local_result));
                    }
                }
            }

            [Command("lua", RunMode = RunMode.Async), Summary("Executes Lua with some restrictions.")]
            public async Task lua(string eval)
            {
                if (!GeneralTools.UserTrusted(Context)) {
                    await ReplyAsync("User is not trusted.");
                    return;
                }

                string local_result;
                int _timeout = 15;

                Process code_handler = Process.Start(Environment.CurrentDirectory + "\\code-handler\\xubot-code-compiler.exe", "lua " + eval);

                string uri = Path.GetTempPath() + "InterpResult.xubot";

                code_handler.WaitForExit(_timeout * 1000);

                if (!code_handler.HasExited)
                {
                    code_handler.Kill();
                    local_result = _timeout + " seconds past w/o result.";
                    await ReplyAsync("", false, BuildEmbed("Lua", "using NLua", "lua", eval, local_result));
                }
                else
                {
                    if (File.Exists(uri))
                    {
                        local_result = File.ReadAllText(uri);
                        File.Delete(uri);

                        await ReplyAsync("", false, BuildEmbed("Lua", "using NLua", "lua", eval, local_result));
                    }
                    else
                    {
                        local_result = "Result was not stored.";
                        await ReplyAsync("", false, BuildEmbed("Lua", "using NLua", "lua", eval, local_result));
                    }
                }
            }

            [Command("powershell", RunMode = RunMode.Async), Summary("Executes Powershell with restrictions.")]
            public async Task ps_sudo(string eval)
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Async = true;
                
                if (GeneralTools.UserTrusted(Context))
                {
                    if (CompileTools.PowershellDangerous(eval) == "")
                    {
                        string local_result;
                        int _timeout = 5;

                        Process psproc = new Process();

                        psproc.StartInfo.UseShellExecute = false;
                        psproc.StartInfo.RedirectStandardOutput = true;
                        psproc.StartInfo.FileName = "powershell.exe";
                        psproc.StartInfo.Arguments = "-Command " + eval;
                        psproc.Start();

                        string psout = psproc.StandardOutput.ReadToEnd();
                        psproc.WaitForExit(_timeout * 1000);

                        if (!psproc.HasExited)
                        {
                            psproc.Kill();
                            local_result = _timeout + " seconds past w/o result.";
                            await ReplyAsync("", false, BuildEmbed("Powershell", "Using Direct Execution", "powershell", eval, local_result));
                        }
                        else
                        {
                            local_result = psout;
                            await ReplyAsync("", false, BuildEmbed("Powershell", "Using Direct Execution", "powershell", eval, local_result));
                        }
                    }
                }
                else
                {
                    await ReplyAsync("Sorry, but this command is restricted.");
                }
            }

            [Command("deadfish", RunMode = RunMode.Async), Summary("Interperts Deadfish and outputs the results.")]
            public async Task deadfish(string eval)
            {
                string local_result = SmallLangInterps.Deadfish.Execute(eval);
                await ReplyAsync("", false, BuildEmbed("Deadfish", "using a built-in interpeter (adapted from https://esolangs.org)", "", eval, local_result));
            }
            
            [Command("brainfuck", RunMode = RunMode.Async), Alias("brainf***", "brainf**k", "b****fuck", "bf"), Summary("Interperts Brainfuck and outputs the result.")]
            public async Task brainfuck(string eval, string ascii_input = "")
            {
                string temp_eval;
                if (ascii_input != "")
                {
                    temp_eval = "Code: " + eval.Replace("\n", String.Empty) + "\n\nASCII Input: " + ascii_input;
                }
                else
                {
                    temp_eval = "Code: " + eval.Replace("\n", String.Empty);
                }
                string local_result = SmallLangInterps.Brainfuck.Execute(eval, ascii_input);

                await ReplyAsync("", false, BuildEmbed("Brainfuck", "using a built-in interpeter (adapted from https://github.com/james1345-1/Brainfuck/)", "bf", temp_eval, local_result));
            }
        }
    }

    public partial class CompileTools
    {
        public static string PowershellDangerous(string input)
        {
            if (input.ToLower().Contains("start-process") || input.ToLower().Contains("invoke-item") ||
                input.ToLower().Contains("ii ") || input.ToLower().Contains("system.diagnostics.process") ||
                input.ToLower().Contains("stop-service") || input.ToLower().Contains("spsv ") ||
                input.ToLower().Contains("start-service") || input.ToLower().Contains("sasv ") ||
                input.ToLower().Contains("restart-service") ||
                input.ToLower().Contains("stop-process") || input.ToLower().Contains("spps ") || input.ToLower().Contains("kill ") ||
                input.ToLower().Contains("suspend-service") || input.ToLower().Contains("resume-service") ||
                input.ToLower().Contains("invoke-expression") || input.ToLower().Contains("iex "))
            {
                return "Starting/closing processess is disallowed.";
            }
            else if (input.ToLower().Contains("keys.json"))
            {
                return "Interacting with my API keys is disallowed.";
            }
            else if (input.ToLower().Contains("delete") || input.ToLower().Contains("remove-item"))
            {
                return "Deleting/removing anything is disallowed.";
            }
            else if (input.ToLower().Contains("rename-item") || input.ToLower().Contains("cpi ") ||
                input.ToLower().Contains("ren "))
            {
                return "Renaming files are disallowed.";
            }
            else if (input.ToLower().Contains("move-item") || input.ToLower().Contains("mi ") ||
                input.ToLower().Contains("mv ") || input.ToLower().Contains("move "))
            {
                return "Moving files are disallowed.";
            }
            else if (input.ToLower().Contains("copy-item") || input.ToLower().Contains("cpi ") ||
                input.ToLower().Contains("cp ") || input.ToLower().Contains("copy "))
            {
                return "Copying files are disallowed.";
            }
            else if (input.ToLower().Contains("stop-computer") || input.ToLower().Contains("restart-computer"))
            {
                return "DA FUCK YOU DOING MATE (╯°□°）╯︵ ┻━┻";
            }
            else if (input.ToLower().Contains("set-date"))
            {
                return "Changing my computer's date is not nice. So stop it.";
            }
            else if (input.ToLower().Contains("get-item") || input.ToLower().Contains("gu ") ||
                input.ToLower().Contains("set-executionpolicy") ||
                input.ToLower().Contains("new-alias") || input.ToLower().Contains("nal ") ||
                input.ToLower().Contains("import-alias") || input.ToLower().Contains("ipal ") ||
                input.ToLower().Contains("get-alias") || input.ToLower().Contains("gal ") ||
                input.ToLower().Contains("set-alias") ||
                input.ToLower().Contains("export-alias") || input.ToLower().Contains("epal "))
            {
                return "No. Besides these are (mostly) based on session.";
            }
            else if (input.ToLower().Contains("clear-content") || input.ToLower().Contains("clc "))
            {
                return "Editing things is disallowed.";
            }
            else if (input.ToLower().Contains("new-item") || input.ToLower().Contains("ni "))
            {
                return "Creating things is disallowed.";
            }
            else if (input.ToLower().Contains("cmd") || input.ToLower().Contains("control") || input.ToLower().Contains("wmic") || input.ToLower().Contains("taskmgr") || input.ToLower().Contains("tasklist") || input.ToLower().Contains("del ") || input.ToLower().Contains("sc ") || input.ToLower().Contains(">"))
            {
                return "Starting executables from PATH to get around blocks (and piping to write a file) is disallowed.";
            }
            else
            {
                return "";
            }
        }
    }

    public partial class SmallLangInterps
    {
        //adapted from https://esolangs.org/wiki/Deadfish#C.23
        public partial class Deadfish
        {
            public static string Execute(string input)
            {
                input = input.Replace(((char)13).ToString(), "");
                string output = "";

                int cell = 0;

                foreach (char c in input)
                {
                    if (c == 'i')
                    {
                        cell++;
                    }
                    else if (c == 'd')
                    {
                        cell--;
                    }
                    else if (c == 's')
                    {
                        int i = cell * cell;
                        cell = i;
                    }
                    else if (c == 'o')
                    {
                        output += cell.ToString();
                    }

                    if (cell == -1 || cell == 256)
                    {
                        cell = 0;
                    }
                }

                return output;
            }
        }

        // adapted from https://github.com/james1345-1/Brainfuck/blob/master/C%23/Brainfuck.cs
        public partial class Brainfuck
        {
            public static string Execute(string input, string ascii_input = "a")
            {
                char[] memory = new char[10000];
                int memory_pointer = 0;

                char[] actions = input.ToCharArray();
                int action_pointer = 0;

                char[] inputs = ascii_input.ToCharArray();
                int input_pointer = 0;

                string output = "";

                while (action_pointer < actions.Length)
                {
                    char _ = actions[action_pointer];

                    switch (_)
                    {
                        case '>':
                            {
                                memory_pointer++;
                                break;
                            }
                        case '<':
                            {
                                memory_pointer--;
                                break;
                            }
                        case '+':
                            {
                                memory[memory_pointer]++;
                                break;
                            }
                        case '-':
                            {
                                memory[memory_pointer]--;
                                break;
                            }
                        case '.':
                            {
                                output += memory[memory_pointer];
                                break;
                            }
                        case ',':
                            try
                            {
                                memory[memory_pointer] = inputs[input_pointer];
                                input_pointer++;
                            }
                            catch (Exception e)
                            {
                                // do nothing
                            }
                            break;
                        case '[':
                            if (memory[memory_pointer] == 0)
                            {
                                while (actions[action_pointer] != ']') action_pointer++;
                            }
                            break;

                        case ']':
                            if (memory[memory_pointer] != 0)
                            {
                                while (actions[action_pointer] != '[') action_pointer--;
                            }
                            break;
                    }

                    // increment instruction mp
                    action_pointer++;
                }
                
                return output;
            }
        }
    }
}
