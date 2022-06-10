using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinAspeak.models
{
    internal class SelectVoices
    {
        /// <summary>
        /// 排序名称
        /// </summary>
        public string? ShortName { get; set; }
        /// <summary>
        /// 样式
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// 语速
        /// </summary>
        public float Rate { get; set; }

        /// <summary>
        /// 音调
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// 输出文件链接
        /// </summary>
        public string? OutputFile { get; set; }

        public override string ToString()
        {
            return
                $"{(Text != null ? $" -t \"{Text}\" " : "")}" +
                $"{(ShortName != null ? $" -v {ShortName} " : "")}" +
                $" -p {Pitch / 0.5 * 0.01 + 1:0.00}" +
                $" -r {(Rate * 0.01) + 1:0.00} " +
                $"{(Style != null ? $" -S {Style} " : "")}" +
                $"{(OutputFile != null ? $" -o \"{OutputFile}\" " : "")}";
        }
    }
}
