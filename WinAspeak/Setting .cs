using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAspeak.models;

namespace WinAspeak
{
    public partial class Setting : Form
    {
        public readonly string JsonFile = "Setting.Json";
        public HubConnection? connection;

        public Setting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗口加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting_Load(object sender, EventArgs e)
        {
            if (!File.Exists(JsonFile))
            {
                return;
            }
            var jsonText = File.ReadAllText(JsonFile);
            var setInfo = JsonConvert.DeserializeObject<SetInfo>(jsonText);
            textBox1.Text = setInfo?.ServerUrl;
            numericUpDown1.Value = setInfo?.CallNumber ?? 1;

            Task.Run(async () =>
            {
                // http://localhost:5190/ChatHub
                connection = new HubConnectionBuilder()
                 .WithUrl(textBox1.Text ?? "")
                 .WithAutomaticReconnect()
                 .Build();
                var onMessage = new Subject<string>();
                // 初始化 SignalR

                //var onMessage = new Subject<string>();
                connection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await connection.StartAsync();
                };

                // 开始
                await connection.StartAsync();

                // 监听
                connection.On<string>("TestConnectionOk", (message) =>
                {
                    // winfroms 使用下面的
                    // wpf版的 将 BeginInvoke 替换成 this.Dispatcher.Invoke
                    BeginInvoke(() =>
                    {
                        label4.Text = "连接成功!";
                    });
                });
            });
         
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var setInfo = new SetInfo()
            {
                ServerUrl = textBox1.Text,
                CallNumber = (int)numericUpDown1.Value,
            };
            var jsonString = JsonConvert.SerializeObject(setInfo);
            Task.Run(async () => await File.WriteAllTextAsync(JsonFile, jsonString));
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("请填写服务器链接!\n例如:http://xxxx.com/ChatHub");
                return;
            }

            label4.Text = "连接失败";

            Task.Run(async() =>
            {
                if(connection != null)
                    await connection.InvokeAsync("TestConnection", "Test");
            });
            //connection.StopAsync();

        }
    }
}
