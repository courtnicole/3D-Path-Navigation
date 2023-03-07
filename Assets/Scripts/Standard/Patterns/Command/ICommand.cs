namespace PathNav.Patterns.Command
{
    public interface ICommand
    {
        bool Execute(object args);
    }
}