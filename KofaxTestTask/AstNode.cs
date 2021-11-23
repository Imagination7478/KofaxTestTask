using System;

namespace KofaxTestTask
{
    public class AstNode
    {
        private AstNode parent;
        private AstNode rChild;
        private AstNode lChild;
        private string text = "";
        private int nodeType;

        public AstNode(string type, AstNode _parent = null, AstNode _rChild = null, AstNode _lChild = null)
        {
            this.parent = _parent;
            this.rChild = _rChild;
            this.lChild = _lChild;
            nodeType = setType(type);
            text = type;
        }
        
        
        public void setText(string text)
        {
            this.text = text;
        }
        public void setRChild(AstNode _rChild)
        {
            this.rChild = _rChild;
        }
        public void setLChild(AstNode _lChild)
        {
            this.lChild = _lChild;
        }
        public int setType(string type)
        {
            switch (type)
            {
                case "*":
                    return AstNodeType.MUL;
                case "/":
                    return AstNodeType.DIV;
                case "+":
                    return AstNodeType.ADD;
                case "-":
                    return AstNodeType.SUB;
                default:
                    return AstNodeType.NUMBER;
            }
        }
        public int getType()
        {
            return nodeType;
        }
        public string getText()
        {
            return this.text;
        }
        public AstNode getParent()
        {
            return this.parent;
        }
        public AstNode getRChild()
        {
            return this.rChild;
        }
        public AstNode getLChild()
        {
            return this.lChild;
        }
    }

    public class AstNodeType
    {
        public const int UNKNOWN = 0;
        public const int NUMBER = 1;
        public const int ADD = 11;
        public const int SUB = 12;
        public const int MUL = 13;
        public const int DIV = 14;
    }
}



