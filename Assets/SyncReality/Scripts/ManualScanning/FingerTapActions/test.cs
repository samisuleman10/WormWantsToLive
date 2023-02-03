using System.Collections;
using System.Collections.Generic;
using UnityEngine;




    public class Parent
    {
        public virtual string Display()
        {
            return "Parent Class!";
        }
    }

    public class Child : Parent
    {
        public override string Display()
        {
            return "Child Class!";
        }
    }




public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var parent = new Parent();
        var child = new Child();

            
        var parent2 = (Parent)child;
        var child2 = (Child)parent2;

        
        Debug.LogError(parent2.Display());
    }

}
