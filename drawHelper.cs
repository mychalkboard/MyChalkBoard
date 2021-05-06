using Godot;
using System;
using System.Collections.Generic;

public class drawHelper : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    
    public override void _Ready()
    {
        
    }

    public static void drawPoints( drawObject _targetNode,List<Vector2> _pointList,List<ImageTexture> _drawTextures,List<int> _brushIndexList,Color brushColor){

        for(int n=0;n<_pointList.Count;n++){
            //Selecting texture
            var drawTexture=_drawTextures[ _brushIndexList[n] ];

            var point=_pointList[n];
            var drawPos=point-(drawTexture.GetSize()*0.5f);
            if(_targetNode.drawMode==drawObjectMode.brush){
                _targetNode.DrawTexture(drawTexture,drawPos,brushColor);
            }else if(_targetNode.drawMode==drawObjectMode.erase){
                _targetNode.DrawTexture(drawTexture,drawPos,brushColor);
            }
            
        }
    }
    

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
