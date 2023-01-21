namespace Utils.UI.Dragging
{
    public interface IDragSource<T> where T : class
    {
        T GetItem();

        int GetNumber();

        SlotType GetSlotType();

        void RemoveItems(int number);
    }
}