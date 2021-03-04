namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    public interface IDeleteStoredData
    {
        void OnDeleteStateChanged(bool deleteState);
        bool HasStoredData { get; }
    }

}