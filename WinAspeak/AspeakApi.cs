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

        /// <summary>
        /// 获取Voices 列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<Voices>> GetVoicesList()
        {
            var (token, region) = await GetValueAsync();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.115 Safari/537.36");
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");

            var streamTask = client.GetStringAsync($"https://{region}.tts.speech.microsoft.com/cognitiveservices/voices/list");
            var a = await streamTask;
            var repositories = JsonConvert.DeserializeObject<List<Voices>>(a);

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


        public string RunExternalExe(string filename, string? arguments = null)
        {
            var process = new Process();

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
                throw new Exception("OS error while executing " + Format(filename, arguments) + ": " + e.Message, e);
            }

            if (process.ExitCode == 0)
            {
                return stdOutput.ToString();
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
