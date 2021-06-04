/* Máster Creación de videojuegos de la Universidad de Málaga. 2020
 *          https://www.mastervideojuegos.uma.es/
 * Antonio José Fernández Leiva @afdezleiva (Twitter)
 * Binary Tree. Example of implementation of generic tree (template). 
 *		Class BinaryTree<T>
  * Date: 2020/05/22
 */

public class BinaryTree<T>
{
    private Node<T> _root;
    private BinaryTree<T> _left;
    private BinaryTree<T> _right;

    public BinaryTree(){
  
    }

    public BinaryTree(T data)
    {
        _root = new Node<T>(data);
        this._left = null;
        this._right = null;
    }

    public BinaryTree(T data, BinaryTree<T> left, BinaryTree<T> right){
        _root = new Node<T>(data);
        this._left = left;
        this._right = right;
    }

    public T Root{
        get{
            return _root.Value;
        }
        set{
            _root.Value = value;
        }
    }

    public BinaryTree<T> Left{
        get{
            return _left;
        }
        set{
            _left = value;
        }
    }

    public BinaryTree<T> Right{
        get{
            return _right;
        }
        set{
            _right = value;
        }
    }

    public bool isAleaf(){
        return ((this.Left==null) && (this.Right == null));
    }

    public bool hasLeftChild()
    {
        return (this.Left != null);
    }

    public bool hasRightChild()
    {
        return (this.Right != null);
    }

    public bool hasTwoChilds(){
        return ((this.Left!=null) && (this.Right != null));
    }

    public override string ToString(){   //It shows the tree in order
        string datastring="";
        if (this!=null){
            if (this._left!=null)
              datastring += "<Left>" + this._left.ToString() + "</Left>";
            
            datastring += "<Root(" + this._root.ToString() + ">";
            
            if (this._right != null)
               datastring += "<Right>" +  this._right.ToString() + "</Right>";
        }

        return string.Format("{0}",datastring);
    }

}
