namespace Darl.Lineage
{
    public interface ILineage
    {
        bool IsChildOff(Lineage other);
        bool IsParentOf(Lineage other);
    }
}