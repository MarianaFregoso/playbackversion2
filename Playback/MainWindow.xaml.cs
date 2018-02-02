﻿using System;
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
            for (int i=0; i<WaveOut.DeviceCount; i++)
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
                    output.Volume = (float)sldvolumen.Value;

                    output.Init(reader);
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
            if (output != null)
                {
                output.Volume = (float)sldvolumen.Value;
                }
        }
    }
}
