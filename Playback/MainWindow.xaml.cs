using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows.Threading;
using System.IO;


namespace Playback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Mp3FileReader reader;
        private WaveOutEvent output;
        DispatcherTimer timer;
        bool dragging = false;
        private VolumeWaveProvider16 volumeprovider;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += OnTimerTick;

            LlenarComboDispositivos();
        }

        private void LlenarComboDispositivos()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capacidades =
                 WaveOut.GetCapabilities(i);
                cbDispositivo.Items.Add(capacidades.ProductName);
            }
            cbDispositivo.SelectedIndex = 0;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (reader != null)
            {
                string tiempoActual = reader.CurrentTime.ToString();
                tiempoActual = tiempoActual.Substring(0, 8);
                lblPosition.Content = tiempoActual;

                if (!dragging)
                {
                    sldPosition.Value = reader.CurrentTime.TotalSeconds;
                }
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                txtRuta.Text = openFileDialog.FileName;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (output != null && output.PlaybackState == PlaybackState.Paused)
            {
                output.Play();
                btnStop.IsEnabled = true;
                btnPause.IsEnabled = true;
                btnPlay.IsEnabled = false;
            }
            else
            {
                if (txtRuta.Text != null && txtRuta.Text != "")
                {
                    output = new WaveOutEvent();
                    output.PlaybackStopped += OnPlaybackStop;
                    reader = new Mp3FileReader(txtRuta.Text);

                    //configuaraciones de waveout
                    output.DeviceNumber = cbDispositivo.SelectedIndex;
                    output.NumberOfBuffers = 2;
                    output.DesiredLatency = 100;

                    volumeprovider = new VolumeWaveProvider16(reader);

                    volumeprovider.Volume = (float)sldvolumen.Value;


                    output.Init(volumeprovider);
                    output.Play();

                    btnStop.IsEnabled = true;
                    btnPause.IsEnabled = true;
                    btnPlay.IsEnabled = false;

                    lblDuration.Content = reader.TotalTime.ToString().Substring(0, 8);
                    lblPosition.Content = reader.CurrentTime.ToString().Substring(0, 8);
                    sldPosition.Maximum = reader.TotalTime.TotalSeconds;
                    sldPosition.Value = 0;
                    
                    timer.Start();
                }
                else
                {
                    //Avisarle al usuario que elija un archivo
                }
            }
        }

        private void OnPlaybackStop(object sender, StoppedEventArgs e)
        {
            reader.Dispose();
            output.Dispose();
            timer.Stop();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                output.Stop();
                btnPlay.IsEnabled = true;
                btnPause.IsEnabled = false;
                btnStop.IsEnabled = false;
            }
        }

        private void sldPosition_dragStarted(object sender, RoutedEventArgs e)
        {
            if (reader != null)
            {
                dragging = true;
            }
        }

        private void sldPosition_dragCompleted(object sender, RoutedEventArgs e)
        {
            if (reader != null && output != null && (output.PlaybackState == PlaybackState.Playing || output.PlaybackState == PlaybackState.Paused))
            {
                reader.CurrentTime = TimeSpan.FromSeconds(sldPosition.Value);
                dragging = false;
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                if (output.PlaybackState == PlaybackState.Playing)
                {
                    output.Pause();
                    btnPlay.IsEnabled = true;
                    btnPause.IsEnabled = false;
                }
            }
        }

        private void sldvolumen_DragCompleted(object sender, RoutedEventArgs e)
        {

        }



        private void sldvolumen_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeprovider != null)
            {
                volumeprovider.Volume =
                    (float)sldvolumen.Value;
            }

        }


    //Extrae del segundo 10 al 20 y lo guarda en archivo nuevo llamado cortado .mp3
       
        private void btnCortar_Click(object sender, RoutedEventArgs e)
        {
            //verififcar que hay ruta 
            if (txtRuta.Text != null && txtRuta.Text != string.Empty)
            {
                var reader = 
                    new Mp3FileReader(txtRuta.Text);
                var writer =
                    File.Create("Cortado.pm3");
                var posicionInicial =
                    TimeSpan.FromSeconds(10);
                var posicionFinal =
                    TimeSpan.FromSeconds(20);

                reader.CurrentTime = posicionInicial;
                while(reader.CurrentTime < posicionFinal)
                {
                    var frame =
                        reader.ReadNextFrame();
                    if (frame == null)
                    {
                        break;
                    }
                    writer.Write(frame.RawData, 0, frame.RawData.Length);
                }
                writer.Dispose();
            }
        }

        //va a generar una señal con una frecuencia de 440
        // y la guardara en un wav 
        private void btncrearfrecuenca_Click(object sender, RoutedEventArgs e)
        {
            var samplerate = 44100;
            var channalcount = 1;
            var signalgenerator =
                new SignalGenerator(samplerate, channalcount);
            signalgenerator.Type = SignalGeneratorType.Sin;
            signalgenerator.Frequency = Int32.Parse(txtfrecuencia.Text);
            signalgenerator.Gain = 0.5;

            var WaveFormat = new WaveFormat(samplerate, 16, channalcount);

            var writer = new WaveFileWriter(txtnombre.Text, WaveFormat);

            var muestrasporsegundo = samplerate * channalcount;

            var buffer = new float[muestrasporsegundo];
            
            for (int i = 0; i < Int32.Parse(txtsegundos.Text); i++)
            {
                var muestras = signalgenerator.Read(buffer, 0, muestrasporsegundo);
                writer.WriteSamples(buffer, 0, muestras);
            }
            writer.Dispose();
        }
    }
}
