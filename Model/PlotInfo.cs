using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VisualHFT.Model
{
    public class PlotInfo 
    {
        public DateTime Date { get; set; }

        double _value;
        public double Value
        {
            get => _value;
            set => _value = value;
        }
    }
}
