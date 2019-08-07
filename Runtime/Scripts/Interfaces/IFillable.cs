using System.Collections.Generic;

public interface IFillable<Titem>
{
    void AddItem(Titem item);
    void AddItems(Titem[] item);
    void RemoveItem(Titem item);
    List<Titem> GetItems();
}
