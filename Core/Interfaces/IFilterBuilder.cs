namespace Kobanan
{
    public interface IFilterBuilder
    {
        IFilter End();
        IFilterBuilder Inc<T>();
        IFilterBuilder Exc<T>();
        IFilterBuilder Des<T>();
        IFilterBuilder Child<T>();
    }
}