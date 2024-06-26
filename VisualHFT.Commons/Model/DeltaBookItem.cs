using VisualHFT.Commons.Pools;
using VisualHFT.Enums;

namespace VisualHFT.Commons.Model
{
    public class DeltaBookItem : IResettable
    {
        public eMDUpdateAction MDUpdateAction { get; set; }
        public string EntryID { get; set; }
        public DateTime LocalTimeStamp { get; set; }
        public DateTime ServerTimeStamp { get; set; }
        public double? Price { get; set; }
        public double? Size { get; set; }
        public bool? IsBid { get; set; }
        private string _symbol;
        public string Symbol
        {
            get => _symbol;
            set => _symbol = StringPool.Shared.GetOrAdd(value); /*using string pool for performance reasons */
        }

        public string OriginalEntryID { get; set; }

        public void Reset()
        {
            MDUpdateAction = eMDUpdateAction.None;
            EntryID = "";
            LocalTimeStamp = DateTime.MinValue;
            ServerTimeStamp = DateTime.MinValue;
            Price = null;
            Size = null;
            IsBid = null;
            _symbol = "";
            OriginalEntryID = "";
        }
    }
}