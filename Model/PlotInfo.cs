using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VisualHFT.Commons.Model;

namespace VisualHFT.Model
{
    public partial class PlotInfo : IResettable
    {
        public DateTime Date { get; set; }

        double _value;
        public double Value
        {
            get => _value;
            set => _value = value;
        }

        public void Reset()
        {
            _value = 0;
            Date = DateTime.MinValue;
        }
    }
}
