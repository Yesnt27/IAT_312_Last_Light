// Something that wants first say when the player uses an item (e.g. the bookshelf puzzle holds the note up
// next to a book). Register with Inventory.AddUseHandler while active; return true to consume the use.
public interface IItemUseHandler
{
    bool TryUse(InventoryItem item);
}
