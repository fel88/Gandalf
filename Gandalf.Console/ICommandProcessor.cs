namespace Gandalf
{
    public interface ICommandProcessor
    {
        Task<bool> Process(string message);
    }
}
