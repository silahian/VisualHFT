using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Collections;
using VisualHFT.Model;

namespace VisualHFT.ViewModel
{
    public class vmPriceCharting
	{
		private ObservableCollection<PlotInfo> asyncData;
		private int maxItemsCount = 1500;

		private Timer timer;
		private int randomizerCount;
		public string SelectedSymbol;
		
		public vmPriceCharting()
		{
			this.asyncData = new ObservableCollection<PlotInfo>();
			Helpers.HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;

			this.PopulateData();
			this.timer = new Timer();
			this.timer.Interval = 50;
			this.timer.Elapsed += Timer_Elapsed;
			this.timer.Start();
		}
		private void PROVIDERS_OnDataReceived(object sender, ProviderVM e)
		{

		}

        public ObservableCollection<PlotInfo> AsyncData => this.asyncData;

        private void PopulateData()
		{
			DateTime now = DateTime.Now;
			for (int i = 0; i < this.maxItemsCount; i++)
			{
				this.asyncData.Add(new PlotInfo { Date = now.AddMilliseconds((i - this.maxItemsCount) * 50), Value = GetNextY() });
			}
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			lock (((ICollection)this.asyncData).SyncRoot)
			{
				DateTime now = DateTime.Now;
				this.asyncData.Add(new PlotInfo { Date = now, Value = this.GetNextY() });

				int count = this.asyncData.Count - this.maxItemsCount;
				for (int i = 0; i < count; i++)
				{
					this.asyncData.RemoveAt(0);
				}
			}
		}

		private int GetNextY()
		{
			this.randomizerCount++;
			int i = this.randomizerCount;
			int y = i % 93 + i % 71 + i % 47 + i % 13 + i % 7;

			return y;
		}
	}
}
