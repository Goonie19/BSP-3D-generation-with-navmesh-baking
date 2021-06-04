/*
 * Máster Creación de videojuegos de la Universidad de Málaga. 2020
 * Antonio José Fernández Leiva
 * Binary Tree. Example of implementation of generic tree (template). 
 *		Class Node<T>
 * */

public class Node <T>
{
    private T _data;

    public Node(){

    }

    public Node(T data){
        this._data = data;
    }

    public T Value{
        get{
            return _data;
        }
        set{
            _data = value;
        }
    }

    public override string ToString(){
        string datastring= (_data==null)?"null":_data.ToString();
        return string.Format("{0}",datastring);
    }
}
