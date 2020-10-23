namespace LccEditor
{
    public class Cell
    {
        public string name;
        public string type;
        public string desc;
        public Cell()
        {
        }
        public Cell(string name, string type, string desc)
        {
            this.name = name;
            this.type = type;
            this.desc = desc;
        }
    }
}