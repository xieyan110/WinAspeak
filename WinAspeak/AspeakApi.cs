using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAspeak.models;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WinAspeak
{
    /// <summary>
    /// 使用要使用需要安装python 
    /// 以及：pip install --upgrade aspeak
    /// 参考：https://github.com/kxxt/aspeak
    /// </summary>
    public class AspeakApi
    {

        private static readonly HttpClient client = new HttpClient();

        public readonly string jsonfile = "Voices.json";
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Voices>> GetVoicesList()
        {
            if (File.Exists(jsonfile))
            {
                var jsonText = await ReadJson();
                var repositories = JsonConvert.DeserializeObject<List<Voices>>(jsonText);

                return repositories ?? new List<Voices>();
            }
            return await GetUrlVoicesList();
        }

        public async Task<string> ReadJson()
        {

            return await File.ReadAllTextAsync(jsonfile);
        }


        /// <summary>
        /// 写入json
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public async Task WriteJson(string jsonText)
        {
            await File.WriteAllTextAsync(jsonfile, jsonText);
        }




        /// <summary>
        /// 获取Voices 列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<Voices>> GetUrlVoicesList()
        {
            var (token, region) = await GetValueAsync();

            client.DefaultRequestHeaders.Accept.Clear();

            client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");

            var streamTask = client.GetStringAsync($"https://{region}.tts.speech.microsoft.com/cognitiveservices/voices/list");
            var jsonText = await streamTask;
            await WriteJson(jsonText);
            var repositories = JsonConvert.DeserializeObject<List<Voices>>(jsonText);

            return repositories ?? new List<Voices>();
        }

        public async Task<(string token, string region)> GetValueAsync()
        {
            var streamTask = client.GetStringAsync("https://azure.microsoft.com/zh-cn/services/cognitive-services/text-to-speech/#overview");
            var input = await streamTask;
            var t = Regex.Match(input, "token:.\"(.+)\"").Groups[1].Value;
            var r = Regex.Match(input, "region:.\"(.+)\"").Groups[1].Value;

            return (t,r);
        }


        public string RunExternalExe(ref Process? process , string filename, string? arguments = null)
        {
            process?.Kill();
            process = new Process();

            process.StartInfo.FileName = filename;
            if (!string.IsNullOrEmpty(arguments))
            {
                process.StartInfo.Arguments = arguments;
            }

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            var stdOutput = new StringBuilder();
            process.OutputDataReceived += (sender, args) => stdOutput.AppendLine(args.Data); // Use AppendLine rather than Append since args.Data is one line of output, not including the newline character.

            string? stdError = null;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                if(e.Message == "No process is associated with this object.")
                {
                    return string.Empty;
                }
                throw new Exception("OS error while executing " + Format(filename, arguments) + ": " + e.Message, e);
            }

            if (process.ExitCode == 0)
            {
                return stdOutput.ToString();
            }
            else if (string.IsNullOrEmpty(stdError) && process.ExitCode == -1)
            {
                // 退出成功
                return "";
            }
            else
            {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError))
                {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0)
                {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }

                throw new Exception(Format(filename, arguments) + " finished with exit code = " + process.ExitCode + ": " + message);
            }
        }

        private string Format(string filename, string? arguments)
        {
            return "'" + filename +
                ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) +
                "'";
        }

    }
}
