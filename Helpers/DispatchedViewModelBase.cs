using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace VisualHFT.Helpers
{
	public class DispatchedViewModelBase : INotifyPropertyChanged
	{
		private HashSet<string> propertyNamesSet;
		private UIActionDispatcher dispatcher;

		public DispatchedViewModelBase()
		{
			this.propertyNamesSet = new HashSet<string>();
			this.dispatcher = new UIActionDispatcher();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void DispatchPropertyChanged(string propertyName)
		{
			lock (this.propertyNamesSet)
			{
				this.propertyNamesSet.Add(propertyName);
			}

			this.dispatcher.Dispatch("RaisePropertyChanged", RaisePropertyChanged);
		}

		protected void DispatchAction(string actionKey, Action action)
		{
			this.dispatcher.Dispatch(actionKey, action);
		}

		private void RaisePropertyChanged()
		{
			List<string> propertyNamesList = new List<string>();
			lock (this.propertyNamesSet)
			{
				propertyNamesList = new List<string>(this.propertyNamesSet);
				this.propertyNamesSet.Clear();
			}

			var h = this.PropertyChanged;
			if (h != null)
			{
				foreach (string propertyName in propertyNamesList)
				{
					h(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}
	}
}
