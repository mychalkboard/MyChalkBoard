using Godot;
using System;

public class helpButton : Button
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public Sprite tutorialSprite;
    public float targetTutorialSpriteAlpha=0;
    public override void _Ready()
    {
        tutorialSprite=(Sprite)Owner.GetNode("tutorial");
    }
    public override void _Process(float delta)
    {
        if(tutorialSprite.SelfModulate.a==targetTutorialSpriteAlpha)return;
        var diff=targetTutorialSpriteAlpha-tutorialSprite.SelfModulate.a;
        if(Mathf.Abs(diff)<0.1){
            tutorialSprite.SelfModulate=new Color(1,1,1,targetTutorialSpriteAlpha);
        }else{
            
            float nAlpha=tutorialSprite.SelfModulate.a+Mathf.Sign(diff)*0.1f;
            tutorialSprite.SelfModulate=new Color(1,1,1,nAlpha);   
        }
    }

    public override void _Pressed(){
        if(targetTutorialSpriteAlpha==0){
            targetTutorialSpriteAlpha=1;
        }else{
            targetTutorialSpriteAlpha=0;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouse mouseEvent && mouseEvent.IsPressed()){
            if(targetTutorialSpriteAlpha==1)targetTutorialSpriteAlpha=0;
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
