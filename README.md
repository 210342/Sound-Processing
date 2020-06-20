# Sound-Processing
A project for an image processing course

Application written in UWP using MVVM design pattern.

Application can read and save sound files in wave format. It can calculate fundamental frequencies for the signal in consecutive windows using Average Magnitude Difference Function or cepstral analysis using real cepstrum. It also implements Finite Impulse Response filters with three different possible windows.

The code depends on:
- [NAudio](https://github.com/naudio/NAudio)
- [Ninject](http://www.ninject.org/)
- [UWPQuickCharts](https://github.com/ailon/UWPQuickCharts)
- [AsyncAwaitBestPractices](https://github.com/brminnick/AsyncAwaitBestPractices)
