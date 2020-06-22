using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;

namespace TestMicro
{
    public partial class Form1 : Form
    {
        //Поток для записи
        WaveIn waveIn;
        //Класс для записи в файл
        WaveFileWriter writer;
        //Создание временного файла
        string outputFilename = "temp_file.wav";
        //Проверка на запись
        bool record = false;
        public Form1()
        {
            InitializeComponent();
        }
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Записываем данные из буфера в файл
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
            }
        }
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }
        public async void SendPostFile(string file)
        {
            try
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        string url = "https://f16petrovtest.herokuapp.com";
                        var httpClient = new HttpClient();
                        MultipartFormDataContent form = new MultipartFormDataContent();
                        form.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(file)), "audio_file", "auido.wav");
                        HttpResponseMessage response = await httpClient.PostAsync(url, form);
                        response.EnsureSuccessStatusCode();
                        httpClient.Dispose();
                        textBox1.Text = response.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (record == false)
            {
                try
                {
                    waveIn = new WaveIn();
                    waveIn.DeviceNumber = 0;
                    //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                    waveIn.DataAvailable += waveIn_DataAvailable;
                    //Формат wav-файла
                    waveIn.WaveFormat = new WaveFormat(16000, 1);
                    writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                    //Начало записи
                    waveIn.StartRecording();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                button1.Text = "Закончить запись";
                record = true;
            }
            else
            {
                if (waveIn != null)
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
                    }
                    else
                    {
                        waveIn.Dispose();
                        waveIn = null;
                        writer.Close();
                        writer = null;
                    }
                }
                button1.Text = "Начать запись";
                record = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendPostFile(outputFilename);
        }
    }
}
