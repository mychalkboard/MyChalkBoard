using Godot;
using System;
using System.Collections.Generic;

public class drawObject : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public List<Vector2> pointList=new List<Vector2>();

    public List<int> brushIndexList=new List<int>();

    List<Texture> brushTextureResources=new List<Texture>();

    List<ImageTexture> brushTextures=new List<ImageTexture>();
    Texture eraseTextureResource;

    ImageTexture drawTexture;

    public bool drawEnded=false;

    public float brushSize=0.08f;

    public Color brushColor=drawController.colorOptions[0];

    public drawObjectMode drawMode;

    public int stepCounter=0;

    public float xMin=999999;
    public float xMax=-999999;
    public float yMin=999999;
    public float yMax=-999999;

    [Signal]
    public delegate void first_draw_ended();

    //Convert Sprite Variables
    public Viewport captureViewport;
    public drawObject cloneDrawObject;
    public Sprite convertedSprite;
    public bool expectedSpriteConvertion=false;
    [Signal]
    public delegate void converted_sprite(Node2D _obj,Sprite _sprite);

    public drawObject(drawObjectMode _drawMode=drawObjectMode.brush){
        drawMode=_drawMode;
        if(_drawMode==drawObjectMode.erase){
            var material=new CanvasItemMaterial();
            material.BlendMode=CanvasItemMaterial.BlendModeEnum.Sub;
            Material=material;
        }        
    }

    public override void _Ready()
    {
        if(drawMode==drawObjectMode.brush){
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/chalk.png") );
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/chalk2.png") );
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/chalk3.png") );
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/chalk4.png") );
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/chalk5.png") );
        }else if(drawMode==drawObjectMode.erase){
            brushTextureResources.Add( (Texture)ResourceLoader.Load("res://textures/erase.png") ); 
        }

        if(drawMode==drawObjectMode.erase)brushSize=0.2f;

        initBrushTextures();

        eraseTextureResource=(Texture)ResourceLoader.Load("res://textures/chalk.png");
        setBrush(drawMode,Color.ColorN("white"),0.1f);
    }

    public void startToConvertSprite(){
        xMin-=64;
        yMin-=64;
        xMax+=64;
        yMax+=64;
        captureViewport=new Viewport();
        captureViewport.Size=new Vector2(xMax-xMin,yMax-yMin);
        captureViewport.RenderTargetUpdateMode=Viewport.UpdateMode.Always;
        captureViewport.RenderTargetClearMode=Viewport.ClearMode.Always;
        captureViewport.TransparentBg=true;
        captureViewport.Usage=Viewport.UsageEnum.Usage2d;
        GetTree().CurrentScene.AddChild(captureViewport);
        cloneDrawObject=new drawObject();
        cloneDrawObject.pointList=pointList;
        cloneDrawObject.brushColor=brushColor;
        cloneDrawObject.brushIndexList=brushIndexList;
        cloneDrawObject.drawMode=drawMode;
        cloneDrawObject.Position-=new Vector2(xMin,yMin);
        captureViewport.AddChild(cloneDrawObject);
        cloneDrawObject.Update();

        cloneDrawObject.Connect("first_draw_ended",this,"finalToConvertSprite");
        
        //GetTree().CreateTimer(0.2f).Connect("timeout",this,"finalToConvertSprite");


    }
    public void finalToConvertSprite(){
        GD.Print("final the convertion");
        GD.Print("first draw");
        var img=captureViewport.GetTexture().GetData();
        img.FlipY();
        var imgTex=new ImageTexture();
        imgTex.CreateFromImage(img);
        var convertedSprite=new Sprite();
        convertedSprite.Texture=imgTex;
        convertedSprite.Centered=false;
        convertedSprite.Position=new Vector2(xMin,yMin);
        if(drawMode==drawObjectMode.brush){
            var material=new ShaderMaterial();
            material.Shader=(Shader)ResourceLoader.Load("res://shaders/blackFix.shader");
            convertedSprite.Material=material;
        }else if(drawMode==drawObjectMode.erase){
            convertedSprite.Material=Material;
        }
        GetParent().AddChild(convertedSprite);
        this.Visible=false;
        EmitSignal("converted_sprite",this,convertedSprite);
        captureViewport.QueueFree();
        cloneDrawObject.QueueFree();

        expectedSpriteConvertion=false;
        this.QueueFree();

    }

    public  void initBrushTextures(){
        for(int i=0;i<brushTextureResources.Count;i++){
            var img=brushTextureResources[i].GetData();
            img.Resize((int)(brushSize*img.GetSize().x),(int)(brushSize*img.GetSize().y) );
            var nTex=new ImageTexture();
            nTex.CreateFromImage(img);
            brushTextures.Add(nTex);
        }
    }

    public override void _Process(float delta)
    {
        stepCounter++;
        if(stepCounter==2){
            EmitSignal("first_draw_ended");
        }
    }

    public override void _Draw(){
        drawHelper.drawPoints(this,pointList,brushTextures,brushIndexList,brushColor);
        
    }

    public void addPoint(Vector2 pos){
        pointList.Add(pos);
        if(pos.x<xMin)xMin=pos.x;
        if(pos.x>xMax)xMax=pos.x;
        if(pos.y<yMin)yMin=pos.y;
        if(pos.y>yMax)yMax=pos.y;
        if(brushTextureResources.Count>1){
            brushIndexList.Add( (new Random() ).Next( 0,brushTextureResources.Count-1  ) );
        }else{
            brushIndexList.Add( 0);
        }
    }
    public void setBrush(drawObjectMode _mode,Color color,float scale=1){
        if(_mode==drawObjectMode.brush){
            var rnd=new Random();
            var chalkImage=brushTextureResources[ rnd.Next(0,brushTextureResources.Count) ].GetData();
            chalkImage.Resize((int)(chalkImage.GetSize().x*scale),(int)(chalkImage.GetSize().y*scale) );
            var chalkTexture=new ImageTexture();
            chalkTexture.CreateFromImage(chalkImage);
            drawTexture=chalkTexture;
        }else if(_mode==drawObjectMode.erase){
            var eraseImage=eraseTextureResource.GetData();
            eraseImage.Resize((int)(eraseImage.GetSize().x*scale),(int)(eraseImage.GetSize().y*scale) );
            var eraseTexture=new ImageTexture();
            eraseTexture.CreateFromImage(eraseImage);
            drawTexture=eraseTexture;
        }
    }
}
public enum drawObjectMode{
    brush,
    erase
}
