using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
	public class UIActionDispatcher
	{
		private static DispatcherTimer timer;
		private static List<Action> actions;
		private static int count;

		private HashSet<string> actionKeys;

		static UIActionDispatcher()
		{
			actions = new List<Action>();

			timer = new DispatcherTimer((DispatcherPriority)8);
			timer.Tick += timer_Tick;
			timer.Interval = TimeSpan.FromMilliseconds(10);
			timer.Start();
		}

		public UIActionDispatcher()
		{
			this.actionKeys = new HashSet<string>();
		}

		public void Dispatch(string actionKey, Action action)
		{
			lock (this.actionKeys)
			{
				if (this.actionKeys.Add(actionKey))
				{
					Action actionToExecute = (Action)(() =>
					{
						lock (this.actionKeys)
						{
							this.actionKeys.Remove(actionKey);
						}
						action();
					});

					lock (actions)
					{
						actions.Add(actionToExecute);
					}
				}
			}
		}

		private static void timer_Tick(object sender, EventArgs e)
		{
			count++;
			if (count % 2 == 0)
			{
				return;
			}

			ExecuteActions();

			timer.Stop();
			timer.Start();
		}

		private static void ExecuteActions()
		{
			List<Action> actionsCopy;
			lock (actions)
			{
				if (actions.Count == 0)
				{
					return;
				}

				actionsCopy = new List<Action>(actions);
				actions.Clear();
			}

			foreach (var action in actionsCopy)
			{
				action();
			}
		}
	}
}
