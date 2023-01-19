namespace SonarGUI
{
    public interface ISonarWindow
    {
        public string WindowId { get; }
        public string WindowTitle { get; }
        public bool Visible { get; }
        public bool Destroy { get; }
        public void Draw();
    }
}
