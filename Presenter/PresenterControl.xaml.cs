﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presenter
{

    /// <summary>
    /// Interaction logic for PresenterControl.xaml
    /// </summary>
    public partial class PresenterControl : Window
    {
        PresenterWindow _window;

        System.Timers.Timer realtimeclk;
        public PresenterControl(PresenterWindow window)
        {
            InitializeComponent();
            _window = window;
            SlideNumView = $"{_window.CurrentSlideNum}/{_window.Slides.Count}";
            _window.OnMediaPlaybackTimeUpdated += _window_OnMediaPlaybackTimeUpdated;
            realtimeclk = new System.Timers.Timer(1000);
            realtimeclk.Elapsed += Realtimeclk_Elapsed;
            realtimeclk.Start();

            // mute playback monitor
            NowSlide.Mute();

            SlideChanged();
        }

        private void Realtimeclk_Elapsed(object sender, ElapsedEventArgs e)
        {
            LocalTime.Dispatcher.Invoke(() =>
            {
                LocalTime.Content = DateTime.Now.ToString("hh:mm:ss tt");
            });
        }

        private void _window_OnMediaPlaybackTimeUpdated(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateLength(e.Length);
            UpdateCurrentTime(e.Current);
            UpdateTimeRemaining(e.Remaining);
        }

        public void UpdateLength(TimeSpan length)
        {
            Media_length.Content = length.ToString("mm\\:ss");
        }

        public void UpdateTimeRemaining(TimeSpan rem)
        {
            Media_rem.Content = rem.ToString("mm\\:ss");
        }

        public void UpdateCurrentTime(TimeSpan cur)
        {
            Media_at.Content = cur.ToString("mm\\:ss");
        }

        string slideNumView = "";
        public string SlideNumView
        {
            get => slideNumView;
            set
            {
                slideNumView = value;
                slidenumview_label.Content = value;
            }
        }

        private void ShowHideMediaControls()
        {
            if (_window.CurrentSlide.type == SlideType.Video)
            {
                // show media controls
                PlaybackControls.Visibility = Visibility.Visible;
                PlaybackTime.Visibility = Visibility.Visible;
                PlaybackTime_1.Visibility = Visibility.Visible;
            }
            else
            {
                // hide media controls
                PlaybackControls.Visibility = Visibility.Hidden;
                PlaybackTime.Visibility = Visibility.Hidden;
                PlaybackTime_1.Visibility = Visibility.Hidden;
            }
        }

        private void SlideChanged()
        {
            SlideNumView = $"{_window.CurrentSlideNum}/{_window.Slides.Count}";
            ShowHideMediaControls();



            UpdatePrevSlide();
            UpdateNowSlide();
            UpdateNextSlide();
            UpdateAfterSlide();
        }

        private void UpdateNowSlide()
        {
            NowSlide.SetMedia(new Uri(_window.CurrentSlide.path), _window.CurrentSlide.type);
        }

        private void UpdatePrevSlide()
        {
            if (_window.CurrentSlideNum - 2 < 0)
            {
                PrevSlide.ShowBlackSource();
            }
            else
            {
                PrevSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum - 2].path), _window.Slides[_window.CurrentSlideNum - 2].type);
            }
        }

        private void UpdateNextSlide()
        {
            if (_window.CurrentSlideNum < _window.Slides.Count)
            {
                NextSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum].path), _window.Slides[_window.CurrentSlideNum].type);
            }
            else
            {
                NextSlide.ShowBlackSource();
            }
        }

        private void UpdateAfterSlide()
        {
            if (_window.CurrentSlideNum + 1 < _window.Slides.Count)
            {
                AfterSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum + 1].path), _window.Slides[_window.CurrentSlideNum + 1].type);
            }
            else
            {
                AfterSlide.ShowBlackSource();
            }
        }


        private void Next_Slide(object sender, RoutedEventArgs e)
        {
            _window.NextSlide();
            SlideChanged();
        }

        private void Prev_Slide(object sender, RoutedEventArgs e)
        {
            _window.PrevSlide();
            SlideChanged();
        }

        private void Play_Media(object sender, RoutedEventArgs e)
        {
            _window.StartMediaPlayback();
            NowSlide.PlayMedia();
        }

        private void Pause_Media(object sender, RoutedEventArgs e)
        {
            _window.PauseMediaPlayback();
            NowSlide.PauseMedia();
        }

        private void Replay_Media(object sender, RoutedEventArgs e)
        {
            _window.RestartMediaPlayback();
            NowSlide.ReplayMedia();
        }

        public event EventHandler OnWindowClosing;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _window.Dispatcher.Invoke(() =>
            {
                OnWindowClosing?.Invoke(this, e);
            });
        }

        private void Reset_Media(object sender, RoutedEventArgs e)
        {
            _window.ResetMediaPlayback();
            NowSlide.ResetMedia();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
