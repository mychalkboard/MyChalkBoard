using Godot;
using System;
using System.Collections.Generic;

public class drawController: Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public bool mouseLeftPressed=false;
    public bool mouseRightPressed=false;


    public board drawBoard;

    public List<Vector2> mousePosTemp=new List<Vector2>();

    public List<drawObject> drawContainer=new List<drawObject>();

    public Node2D drawContainerNode;

    public Viewport drawContainerViewport=null;

    public drawObject lastDrawObject=null;

    public Vector2 lastDrawPoint=Vector2.Left;

    public TextureRect background;

    public static List<Color> colorOptions=new List<Color>();

    public static int currentColorIndex;

    [Signal]
    public delegate void colorChanged(int _colorIndex);

    public List<drawObject> undoList=new List<drawObject>();

    public Button helpButton;
    public override void _Ready()
    {
        drawContainerNode=(Node2D)Owner.GetNode("canvasViewportContainer/canvasViewport/drawContainer");
        drawContainerViewport=(Viewport)Owner.GetNode("canvasViewportContainer/canvasViewport");
        background=(TextureRect)Owner.GetNode("background");
        helpButton=(Button)Owner.GetNode("helpButton");


        //COLOR OPTIONS
        colorOptions.Add (new Color("#ffffff") );
        colorOptions.Add (new Color("#de8787") );
        colorOptions.Add (new Color("#d3b669") );
        colorOptions.Add (new Color("#86c97d") );
        colorOptions.Add (new Color("#7da5c5") );

        CallDeferred("initChalkIcons");

        GetViewport().Connect("size_changed",this,"onViewportSizeChanged");

        onViewportSizeChanged();

        CallDeferred("loadLastDrawings");
        

    }

    public void onViewportSizeChanged(){
        drawContainerViewport.Size=GetViewport().Size;
        background.SetSize(GetViewport().Size);
        var hbPos=new Vector2( 16,GetViewport().Size.y-helpButton.RectSize.y-16);
        helpButton.SetPosition(hbPos);
    }

    public void initChalkIcons(){
        var startPosition=new Vector2(0,175);
        var vSpace=30.0f;
        PackedScene chalkIconScn=(PackedScene) ResourceLoader.Load("res://Nodes/chalkIcon.tscn");
        for(int i=0;i<colorOptions.Count;i++){
            var color=colorOptions[i];
            var chalkIcon=(chalkButton)chalkIconScn.Instance();
            var cSprite=(Sprite)chalkIcon.GetNode("chalk_icon");
            cSprite.SelfModulate=colorOptions[i];
            
            var posX=currentColorIndex==i ? 20.0f:0.0f;
            chalkIcon.Position=startPosition+new Vector2(posX,vSpace*i);
            chalkIcon.colorIndex=i;

            Owner.AddChild(chalkIcon);
        }
    }
    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseEvent){
            var isOnButton=helpButton.GetRect().HasPoint(mouseEvent.Position);
            var btnIndex=(ButtonList)mouseEvent.ButtonIndex;
            if(btnIndex==ButtonList.Left && !isOnButton && !mouseRightPressed){
                mouseLeftPressed=true;
                GD.Print("basildi"); 
                if(mouseEvent.IsPressed()){
                    enterDraw();
                }else{
                    endDraw();
                    GD.Print("birakildi");
                    mouseLeftPressed=false;
                }
            }
            if(btnIndex==ButtonList.Right && !mouseLeftPressed){
                mouseRightPressed=true;
                GD.Print("sag tiklandi");
                if(mouseEvent.IsPressed()){
                    enterErase();
                }else{
                    endErase();
                    GD.Print("sag birakildi");
                    mouseRightPressed=false;
                }
            }
            

        }
        if(@event is InputEventMouseMotion){
            Update();
        }
        if(@event is InputEventKey keyEvent){
            if(keyEvent.IsActionPressed("redo")){
                redo();
            }else if(keyEvent.IsActionPressed("undo")){
                undo();
            }else if(keyEvent.IsActionPressed("clear")){
                clearBoard();

            }else if(keyEvent.IsActionPressed("prev_color")){
                currentColorIndex--;
                if(currentColorIndex<0)currentColorIndex=colorOptions.Count-1;
                EmitSignal("colorChanged",currentColorIndex);
                
            }else if(keyEvent.IsActionPressed("next_color")){
                currentColorIndex++;
                if(currentColorIndex>colorOptions.Count-1)currentColorIndex=0;
                EmitSignal("colorChanged",currentColorIndex);
            }

        }
        
    }

    public void saveLastDrawings(){
        var img=drawContainerViewport.GetTexture().GetData();
        img.FlipY();
        GD.Print(img.SavePng("user://lastDraw.png") );
    }
    public void setEmptyLastDrawings(){
        var img=new Image();
        img.Create(100,100,false,Image.Format.Rgba8);
        GD.Print(img.GetWidth());
        GD.Print(img.SavePng("user://lastDraw.png") );
    }

    public void loadLastDrawings(){
        var img=new Image();
        var err=img.Load("user://lastDraw.png");
        if(err==Error.Ok){
            var imgTex=new ImageTexture(); 
            imgTex.CreateFromImage(img);
            var spr=new Sprite();
            spr.Centered=false;
            var shader=(Shader)ResourceLoader.Load("res://shaders/blackFix.shader");
            var material=new ShaderMaterial();
            material.Shader=shader;
            spr.Material=material;

            spr.Material=material;
            spr.Texture=imgTex;
            drawContainerNode.AddChild(spr);
        }else{
            GD.PrintErr(err);
        }
    }
    public override void _Draw(){
        Vector2 mPos=GetViewport().GetMousePosition();
        if(mouseLeftPressed || mouseRightPressed){
            mousePosTemp.Add(mPos);
            if(mousePosTemp.Count>1){
                var prevPos=mousePosTemp[mousePosTemp.Count-2];
                var bridgeVector=prevPos-mPos;
                var unit=bridgeVector.Normalized();
                var len=bridgeVector.Length();
                var spacing=1;
                for(int n=0;n<(int)len;n+=spacing){
                    var drawPoint=mPos+unit*n;
                    lastDrawObject.addPoint(drawPoint);
                    lastDrawObject.Update();
                }
            }else{
                lastDrawObject.addPoint(mPos);
            }

        }

                
    }

    

    public void endDraw(){
        if(lastDrawObject==null)return;
        lastDrawObject.Update();
        lastDrawObject.drawEnded=true;
        lastDrawObject=null;
        mousePosTemp.Clear();
        saveLastDrawings();

    }

    public void endErase(){
        endDraw();
    }

    public void enterDraw(){
        clearUndo();
        lastDrawObject=new drawObject();
        lastDrawObject.brushColor=colorOptions[currentColorIndex];
        drawContainer.Add(lastDrawObject);
        drawContainerNode.AddChild(lastDrawObject);
    }

    public void enterErase(){
        clearUndo();
        lastDrawObject=new drawObject(drawObjectMode.erase);
        drawContainer.Add(lastDrawObject);
        drawContainerNode.AddChild(lastDrawObject);
    }

    public void clearBoard(){
        endDraw();
        foreach(Node2D obj in drawContainerNode.GetChildren()){
            obj.QueueFree();
        }
        drawContainer.Clear();
        clearUndo();
        setEmptyLastDrawings();
    }

    public void undo(){
        if(drawContainer.Count==0)return;
        var dObj=drawContainer[drawContainer.Count-1];
        undoList.Add(dObj);
        drawContainerNode.RemoveChild(dObj);
        drawContainer.RemoveAt(drawContainer.Count-1);
    }
    public void redo(){
        if(undoList.Count==0)return;
        var dObj=undoList[undoList.Count-1];
        drawContainer.Add(dObj);
        drawContainerNode.AddChild(dObj);
        undoList.RemoveAt(undoList.Count-1);
    }

    public void clearUndo(){
        foreach(drawObject obj in undoList){
            obj.QueueFree();
        }
        undoList.Clear();

    }

    
    

    public enum brushType{
        draw,
        erase
    }
     

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
