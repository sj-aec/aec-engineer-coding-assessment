namespace TransactionExecution;

public interface IModelGateway
{
    string? GetParameter(string elementId, string parameterName);

    void SetParameter(string elementId, string parameterName, string newValue);
}

public interface IModelTransactionFactory
{
    IModelTransaction Begin(string name);
}

public interface IModelTransaction : IDisposable
{
    void Commit();

    void Rollback();
}
