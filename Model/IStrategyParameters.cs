namespace VisualHFT.Model
{
    public interface IStrategyParameters
	{
		bool IsStrategyOn { get; set; }
		string Symbol { get; set; }
		string LayerName { get; set; }
	}
}
