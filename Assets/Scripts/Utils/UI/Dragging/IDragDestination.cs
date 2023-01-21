namespace Utils.UI.Dragging
{
    public interface IDragDestination<T> where T : class
    {
        int MaxAcceptable(T item, SlotType area);

        void AddItems(T item, int number);

        bool IsItemStackable();

        void FlushItem();
    }
}