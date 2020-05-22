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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SoundProcessing.Controls
{
    public sealed partial class TabContentView : UserControl
    {
        public TabContentViewModel ViewModel => DataContext as TabContentViewModel;
        public IEnumerable<double> Tmp { get; } = new double[] { 0, 2, 1, 8, 6, 4, 5 };

        public TabContentView()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int a = 0;
        }
    }
}
