using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace VisualHFT.Helpers
{
	public class DispatchedObservableCollection : IEnumerable, INotifyCollectionChanged
	{
		private IEnumerable asyncCollection;
		private List<object> list;
		private List<NotifyCollectionChangedEventArgs> pendingChanges;
		private UIActionDispatcher dispatcher;

		public DispatchedObservableCollection(object collection)
		{
			this.asyncCollection = (IEnumerable)collection;
			this.list = new List<object>();
			this.pendingChanges = new List<NotifyCollectionChangedEventArgs>();
			this.dispatcher = new UIActionDispatcher();

			foreach (var item in this.asyncCollection)
			{
				this.list.Add(item);
			}
			((INotifyCollectionChanged)collection).CollectionChanged += this.AsyncCollection_CollectionChanged;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		private void AsyncCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			lock (this.pendingChanges)
			{
				this.pendingChanges.Add(e);
			}

			this.dispatcher.Dispatch("Sync", this.Sync);
		}

		private void Sync()
		{
			List<NotifyCollectionChangedEventArgs> pendingChangesCopy;
			lock (this.pendingChanges)
			{
				pendingChangesCopy = new List<NotifyCollectionChangedEventArgs>(this.pendingChanges);
				this.pendingChanges.Clear();
			}

			foreach (var args in pendingChangesCopy)
			{
				this.Update(args);
			}
		}

		private void Update(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					this.list.Insert(args.NewStartingIndex, args.NewItems[0]);
					break;
				case NotifyCollectionChangedAction.Move:
					throw new NotImplementedException();
				case NotifyCollectionChangedAction.Remove:
					this.list.RemoveAt(args.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException();
				case NotifyCollectionChangedAction.Reset:
					throw new NotImplementedException();
				default:
					throw new NotImplementedException();
			}

			this.RaiseCollectionChanged(args);
		}

		private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				collectionChanged(this, args);
			}
		}
	}
}
