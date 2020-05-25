using SoundProcessing.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SoundProcessing.Controls
{
    public sealed partial class MainMDI : UserControl
    {
        public MainMDIViewModel ViewModel => DataContext as MainMDIViewModel;

        public MainMDI()
        {
            DataContext = ViewModelLocator.Container.Value.GetViewModel<MainMDIViewModel>();
            this.InitializeComponent();
        }

        private void MdiArea_TabCloseRequested(muxc.TabView sender, muxc.TabViewTabCloseRequestedEventArgs args)
        {
            ViewModel.CloseTab(args.Tab.DataContext);
        }
    }
}
