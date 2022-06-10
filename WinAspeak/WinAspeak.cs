using WinAspeak.models;

namespace WinAspeak
{
    public partial class WinAspeak : Form
    {
        List<Voices>? voices { get; set; }

        public WinAspeak()
        {
            InitializeComponent();
        }
        private async void WinAspeak_Load(object sender, EventArgs e)
        {
            var api = new AspeakApi();
            voices = await api.GetVoicesList();
            comboBox3.DataSource = voices.Select(x => x.Locale).Distinct().OrderByDescending(x => x.Contains("zh-")).ToList();
            comboBox2.DataSource = voices.Where(x => x.Locale == (string)comboBox3.SelectedValue).Select(x => x.ShortName).ToList();
            comboBox1.DataSource = voices.Where(x => x.ShortName == (string)comboBox2.SelectedValue).Select(x => x.StyleList).FirstOrDefault();

        }

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
            label6.Text = $"{(trackBar2.Value * 0.01) + 1:0.00}";
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label7.Text = $"{trackBar1.Value / 0.5 * 0.01 + 1:0.00}";
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
       
            var select = new SelectVoices()
            {
                ShortName = (string)comboBox2.SelectedValue,
                Style = (string)comboBox1.SelectedValue,
                Rate = trackBar2.Value,
                Pitch = trackBar1.Value,
                Text = textBox1.Text,
                OutputFile = @"C:\Users\TXHZ-C021\Desktop\output.mp3"
            };
            var api = new AspeakApi();
            api.RunExternalExe("aspeak", select.ToString());
            MessageBox.Show("ת���ɹ�!");
        }

        /// <summary>
        /// �����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

        }


    }
}