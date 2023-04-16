namespace LccHotfix
{
    public class MessageAttribute: AttributeBase
    {
        public int Opcode
        {
            get;
        }

        public MessageAttribute(int opcode)
        {
            this.Opcode = opcode;
        }
    }
}