using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
	public interface IStrategyParameters
	{
		bool IsStrategyOn { get; set; }
		string Symbol { get; set; }
		string LayerName { get; set; }
	}
}
