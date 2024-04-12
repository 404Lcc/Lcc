namespace LccEditor
{
    public class Cell
    {
        public string attribute;
        public string name;
        public string type;
        public string desc;
        public Cell()
        {
        }
        public Cell(string attribute, string name, string type, string desc)
        {
            this.attribute = attribute;
            this.name = name;
            this.type = type;
            this.desc = desc;
        }
    }
}