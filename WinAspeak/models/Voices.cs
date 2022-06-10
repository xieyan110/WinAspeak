using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinAspeak.models
{
    public class Voices
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string? Gender { get; set; }
        /// <summary>
        /// 本地名称
        /// </summary>
        public string? LocalName { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string? Locale { get; set; }

        /// <summary>
        /// ShortName
        /// </summary>
        public string? ShortName { get; set; }
        /// <summary>
        /// 样式
        /// </summary>
        public List<string>? StyleList { get; set; }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<string>? RolePlayList { get; set; }

        /// <summary>
        /// 声音类型
        /// </summary>
        public string? VoiceType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 采样率赫兹
        /// </summary>
        public int SampleRateHertz { get; set; }

    }
}
