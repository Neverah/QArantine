namespace QArantine.Code.QArantineGUI.Models
{
    public class WindowSettings
    {
        public string WindowTitle { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public WindowSettings(string windowTitle, double width, double height, int posX, int posY)
        {
            WindowTitle = windowTitle;
            Width = width;
            Height = height;
            PosX = posX;
            PosY = posY;
        }
        
    }
}