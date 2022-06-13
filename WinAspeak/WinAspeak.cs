using System.Diagnostics;
using WinAspeak.models;

namespace WinAspeak
{
    public partial class WinAspeak : Form
    {
        List<Voices>? voices { get; set; }
        public Process? process;

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
            });
        }




        #region  ����

        /// <summary>
        /// ѡ�����Ժ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.DataSource = voices?.Where(x => x.Locale == (string)comboBox3.SelectedValue).Select(x => x.ShortName).ToList();
            comboBox1.DataSource = voices?.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();
        }

        /// <summary>
        /// ѡ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.DataSource = voices?.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            label6.Text = $"{(trackBar2.Value * 0.01):0.00}";
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label7.Text = $"{trackBar1.Value * 0.01:0.00}";
        }

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((string)comboBox3.SelectedValue))
            {
                MessageBox.Show("���ڳ�ʼ�����Ե�һ��!");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("��Ҫ��дת�����ı�!");
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
                    try
                    {
                        Task.Run(() =>
                        {
                            api.RunExternalExe(ref process, "aspeak", select.ToString());
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                });
            });


            //MessageBox.Show("ת���ɹ�!");
        }

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((string)comboBox3.SelectedValue))
            {
                MessageBox.Show("���ڳ�ʼ�����Ե�һ��!");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("��Ҫ��дת�����ı�!");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "���� Mp3|*.mp3";
            saveFileDialog1.Title = "������Ƶ�ļ�";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                Task.Run(async () =>
                {
                    BeginInvoke(async () =>
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
                        try
                        {
                            await Task.Run(() =>
                            {
                                api.RunExternalExe(ref process, "aspeak", select.ToString());
                                MessageBox.Show("ת���ɹ�!");
                            });
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
        /// ֹͣ
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



        #region
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var set = new Setting();
            set.ShowDialog();
        }

        #endregion
    }
}