using AsyncAwaitBestPractices.MVVM;
using SoundManipulation;
using SoundManipulation.Filtering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ViewModels.Controllers;
using ViewModels.DependencyInjection;
using ViewModels.Model;

namespace ViewModels
{
    public class MainMDIViewModel : ViewModel
    {
        private readonly IDispatcher _dispatcher;

        public IWave SelectedWave => SelectedTab?.Wave?.Value;

        #region Observable properties
        public ObservableCollection<TabContentViewModel> Contents { get; } = new ObservableCollection<TabContentViewModel>();

        private TabContentViewModel _selectedTab;

        public TabContentViewModel SelectedTab 
        { 
            get { return _selectedTab; } 
            set
            { 
                _selectedTab = value; 
                OnPropertyChanged(); 
            }
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        #endregion

        public IIOSoundController IOController { get; }

        public MainMDIViewModel(IIOSoundController controller, IDispatcher dispatcher)
        {
            Name = nameof(MainMDIViewModel);
            _dispatcher = dispatcher;
            IOController = controller;
        }

        public bool? CanSaveCurrentSound() => IOController?.CanSaveSoundFile(SelectedWave);

        public Task SaveCurrentSound() => IOController?.SaveSoundFile(SelectedWave);

        public async Task OpenSoundFile()
        {
            TitledObject<IWave> wave = await IOController?.OpenSoundFile();
            if (wave != null)
            {
                SelectTab(AddTab(wave));
            }
        }

        public void CloseTab(object tab)
        {
            Contents.Remove(tab as TabContentViewModel);
            SelectedIndex = Math.Min(SelectedIndex, Contents.Count - 1);
            SelectedTab = SelectedIndex < 0 ? null : Contents.ElementAt(SelectedIndex);
        }

        public async Task CalculatePeriodWithAMDF(int windowSize, double accuracy)
        {
            if (SelectedWave is null)
            {
                return;
            }

            SelectedTab.WindowSize = windowSize;
            SelectedWave.FundamentalFrequencies =
                await Task.Run(() => SelectedTab?.Wave?.Value?.GetFrequencies(
                    nameof(SelectedTab.Wave.Value.AMDF),
                    windowSize,
                    accuracy
                ));
        }

        public async Task CalculatePeriodWithCepstralAnalysis(int windowSize)
        {
            if (SelectedWave is null)
            {
                return;
            }

            SelectedTab.WindowSize = windowSize;
            SelectedWave.FundamentalFrequencies = 
                await Task.Run(() => SelectedTab.Wave.Value.GetFrequencies(
                    nameof(SelectedTab.Wave.Value.CepstralAnalysis), 
                    windowSize, 
                    0
                ));
            
            AddTab(new TitledObject<IWave>(
                SelectedWave.FourierTransform,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            AddTab(new TitledObject<IWave>(
                SelectedWave.WindowedCepstrum,
                $"{SelectedTab.Wave.Title} - Cepstrum"
            ));
        }

        public async Task CalculateFourierTransform()
        {
            if (SelectedWave is null)
            {
                return;
            }

            IWave newWave = await Task.Run(() => SelectedWave.FourierTransform);
            var newTab = AddTab(new TitledObject<IWave>(
                newWave,
                $"{SelectedTab.Wave.Title} - Fourier"
            ));
            SelectTab(newTab);
        }

        public async Task ShowSignalWithFundamentalFrequencies(int windowSize)
        {
            if (SelectedWave?.FundamentalFrequencies is null
                || !(SelectedWave?.FundamentalFrequencies.Any() ?? false))
            {
                return;
            }
            IWave fundamentalWave = await Task.Run(() => SelectedWave.GenerateWaveOfFundamentalFrequencies(windowSize));
            var newTab = AddTab(new TitledObject<IWave>(fundamentalWave, $"Fundamental wave of {SelectedTab.Wave.Title}"));
            SelectTab(newTab);
        }

        public async Task FilterSignal(IFilter filter, FourierWindow window, int hopSize)
        {
            if (SelectedWave is null)
            {
                return;
            }
            string baseTitle = SelectedTab.Wave.Title;
            // filter by convolution
            Task convolutionFiltered = Task.Factory.StartNew(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                IWave wave = SelectedWave.Convolve(WaveFactory.GetFilterWave(filter, SelectedWave.SampleRate));
                stopwatch.Stop();
                await _dispatcher.RunAsync(() =>
                {
                    AddTab(new TitledObject<IWave>(wave, $"{baseTitle} filtered by convolution: {stopwatch.ElapsedMilliseconds} ms"));
                    return Task.CompletedTask;
                });
            }).Unwrap();

            // filter by fourier
            Task fourierFiltered = Task.Factory.StartNew(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                IWave wave = SelectedWave.ApplyFilterByWindowedFourier(filter, window, hopSize);
                stopwatch.Stop();
                await _dispatcher.RunAsync(() =>
                {
                    AddTab(new TitledObject<IWave>(wave, $"{baseTitle} filtered by Fourier: {stopwatch.ElapsedMilliseconds} ms"));
                    return Task.CompletedTask;
                });
            }).Unwrap();
            await Task.WhenAll(convolutionFiltered, fourierFiltered);
        }

        private TabContentViewModel AddTab(TitledObject<IWave> wave)
        {
            TabContentViewModel newContent = new TabContentViewModel(wave, _dispatcher);
            Contents.Add(newContent);
            return newContent;
        }

        private void SelectTab(TabContentViewModel tab)
        {
            SelectedTab = tab;
            SelectedIndex = Contents.IndexOf(tab);
        }
    }
}
