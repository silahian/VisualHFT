namespace VisualHFT.Commons.Model
{
    public interface ICopiable<T>
    {
        public void CopyFrom(T sourceObj);
    }
}