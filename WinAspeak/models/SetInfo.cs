using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAspeak.models
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class SetInfo
    {
        /// <summary>
        /// 服务url
        /// </summary>
        public string? ServerUrl { get; set; }

        /// <summary>
        /// 呼叫次数
        /// </summary>
        public int CallNumber { get; set; }

    }
}
