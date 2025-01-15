namespace VisualHFT.AnalyticReport
{
    public class ScatterChartData
    {
        public ScatterChartData(double x, double y, string toolTip)
        {
            this.XValue = x;
            this.YValue = y;
            this.ToolTip = toolTip;
        }

        public double XValue { get; set; }
        public double YValue { get; set; }

        public string Brush
        {
            get
            {
                if (this.YValue < 0)
                {
                    return "Red";
                }
                else
                {
                    return "Green";
                }
            }
        }
        public string ToolTip { get; set; }
    }

}
