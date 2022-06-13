using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Media;
using System.Reactive.Subjects;
using System.Reactive;
using WinAspeak.models;
using System.Reactive.Linq;

namespace WinAspeak
{
    public partial class WinAspeak : Form
    {
        List<Voices>? voices { get; set; }
        public Process? process;
        public readonly string JsonFile = "Setting.Json";
        public HubConnection? connection;

        public SetInfo? setInfo { get; set; }

        public WinAspeak()
        {
            InitializeComponent();
        }
        private void WinAspeak_Load(object sender, EventArgs e)
        {
            BeginInvoke(async () =>
            {
                var api = new AspeakApi();
                voices = await api.GetVoicesList();
                comboBox3.DataSource = voices.Select(x => x.Locale).Distinct().OrderByDescending(x => x.Contains("zh-")).ToList();
                comboBox2.DataSource = voices.Where(x => x.Locale == (string)comboBox3.SelectedValue).Select(x => x.ShortName).ToList();
                comboBox1.DataSource = voices.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();
                var jsonText = File.ReadAllText(JsonFile);
                setInfo = JsonConvert.DeserializeObject<SetInfo>(jsonText);
                await Task.Run(async () =>
                {
                    #region SignalR
                    // http://localhost:5190/ChatHub
                    connection = new HubConnectionBuilder()
                     .WithUrl(setInfo?.ServerUrl ?? "")
                     .WithAutomaticReconnect()
                     .Build();
                    var onMessage = new Subject<string>();
                    // 初始化 SignalR

                    connection.Closed += async (error) =>
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        await connection.StartAsync();
                    };

                    // 开始
                    await connection.StartAsync();

                    // 监听
                    connection.On<string>("CallMessageOK", (message) =>
                    {
                        onMessage.OnNext(message);
                    });

                    #endregion

                    #region  RX

                    var oldMessage = string.Empty;

                    onMessage
                    .Subscribe(message =>
                    {
                        if (oldMessage == message && File.Exists("output.wav"))
                        {
                            Task.Run(() =>
                            {
                                foreach (var i in Enumerable.Range(1, setInfo?.CallNumber ?? 1))
                                {
                                    SoundPlayer player = new SoundPlayer();
                                    player.SoundLocation = @"output.wav";
                                    player.Load(); //同步加载声音
                                    player.Play(); //启用新线程播放
                                }
                            });
                            return;
                        }

                        oldMessage = message;
                        var callMessage = string.Empty;
                        foreach (var i in Enumerable.Range(1, setInfo?.CallNumber ?? 1))
                            callMessage += message;

                        BeginInvoke(() =>
                        {
                            var select = new SelectVoices()
                            {
                                ShortName = (string)comboBox2.SelectedValue,
                                Style = (string)comboBox1.SelectedValue,
                                Rate = trackBar2.Value,
                                Pitch = trackBar1.Value,
                                Text = callMessage,
                                OutputFile = "output.wav",
                            };
                            var api = new AspeakApi();

                            try
                            {
                                api.RunExternalExe(ref process, "aspeak", select.ToString());
                                foreach (var i in Enumerable.Range(1, setInfo?.CallNumber ?? 1))
                                {
                                    SoundPlayer player = new SoundPlayer();
                                    player.SoundLocation = @"output.wav";
                                    player.Load(); //同步加载声音
                                    player.Play(); //启用新线程播放
                                }

                            }
                            catch (Exception ex)
                            {
                                oldMessage = string.Empty;
                                MessageBox.Show(ex.Message);
                                return;
                            }

                        });

                    });
                    #endregion
                });
            });

        }

        #region  语音

        /// <summary>
        /// 选择语言后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.DataSource = voices?.Where(x => x.Locale == (string)comboBox3.SelectedValue).Select(x => x.ShortName).ToList();
            comboBox1.DataSource = voices?.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();
        }

        /// <summary>
        /// 选择语音后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.DataSource = voices?.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();
        }

        /// <summary>
        /// 语速
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            label6.Text = $"{(trackBar2.Value * 0.01):0.00}";
        }

        /// <summary>
        /// 音调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label7.Text = $"{trackBar1.Value * 0.01:0.00}";
        }

        /// <summary>
        /// 播放事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((string)comboBox3.SelectedValue))
            {
                MessageBox.Show("正在初始化请稍等一会!");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("需要填写转换的文本!");
                return;
            }
            Task.Run(() =>
            {
                BeginInvoke(() =>
                {
                    var select = new SelectVoices()
                    {
                        ShortName = (string)comboBox2.SelectedValue,
                        Style = (string)comboBox1.SelectedValue,
                        Rate = trackBar2.Value,
                        Pitch = trackBar1.Value,
                        Text = textBox1.Text,
                    };
                    var api = new AspeakApi();
                    Task.Run(() =>
                    {
                        try
                        {
                            api.RunExternalExe(ref process, "aspeak", select.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                    });

                });

            });


            //MessageBox.Show("转换成功!");
        }

        /// <summary>
        /// 保存事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((string)comboBox3.SelectedValue))
            {
                MessageBox.Show("正在初始化请稍等一会!");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("需要填写转换的文本!");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "音乐 Mp3|*.mp3";
            saveFileDialog1.Title = "保存音频文件";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                Task.Run(async () =>
                {
                    var select = new SelectVoices()
                    {
                        ShortName = (string)comboBox2.SelectedValue,
                        Style = (string)comboBox1.SelectedValue,
                        Rate = trackBar2.Value,
                        Pitch = trackBar1.Value,
                        Text = textBox1.Text,
                        OutputFile = saveFileDialog1.FileName,
                    };
                    var api = new AspeakApi();
                    await Task.Run(() =>
                    {
                        try
                        {
                            api.RunExternalExe(ref process, "aspeak", select.ToString());
                            MessageBox.Show("转换成功!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }

                    });
                });
            }

        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                process?.Kill();
            });
        }

        #endregion


        #region 设置
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var set = new Setting();
            set.ShowDialog();
        }

        #endregion


        /// <summary>
        /// 测试之后删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                if (connection != null)
                    await connection.InvokeAsync("CallMessage", textBox1.Text);
            });
        }
    }
}