namespace RuntimeTerrainEditor
{    
    public interface ICommand
    {
        void Complete();
        void Execute();
        void Undo();
    }
}