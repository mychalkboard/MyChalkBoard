using Godot;
using System;

public class chalkButton : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.,
    drawController controller;

    public int colorIndex=0;
    public override void _Ready()
    {
        controller=(drawController)GetParent().GetNode("drawController");

        controller.Connect("colorChanged",this,nameof(onColorChanged) );
    }

    public void onColorChanged(int _colorIndex){
        if(_colorIndex==colorIndex){
            Position=new Vector2(20,Position.y);
        }else{
            Position=new Vector2(0,Position.y);
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
