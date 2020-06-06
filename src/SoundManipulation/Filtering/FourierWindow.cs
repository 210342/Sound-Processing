using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SoundManipulation.Filtering
{
    public delegate double WindowDelegate(int sampleIndex, int windowSize); 

    public class FourierWindow
    {
        #region Window delegates

        public static double Rectangular(int sampleIndex, int windowSize)
        {
            return 1.0;
        }

        public static double VonHann(int sampleIndex, int windowSize)
        {
            return 0.5 - 0.5 * Math.Cos(2 * Math.PI * sampleIndex / (windowSize - 1));
        }

        public static double Hamming(int sampleIndex, int windowSize)
        {
            double value = 0.54 - 0.46 * Math.Cos(2 * Math.PI * sampleIndex / (windowSize - 1));
            return sampleIndex == 0 || sampleIndex == windowSize - 1 ? value / 2 : value;
        }

        #endregion

        public WindowDelegate Function { get; }
        public int Length { get; }
        public IEnumerable<double> Values { get; }

        public FourierWindow(WindowDelegate windowFunction, int windowLength)
        {
            Function = windowFunction;
            Length = windowLength;
            Values = Enumerable.Range(0, windowLength).Select(i => windowFunction(i, windowLength));
        }
    }
}
