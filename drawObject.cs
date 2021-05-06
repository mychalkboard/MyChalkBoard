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

    public  void initBrushTextures(){
        for(int i=0;i<brushTextureResources.Count;i++){
            var img=brushTextureResources[i].GetData();
            img.Resize((int)(brushSize*img.GetSize().x),(int)(brushSize*img.GetSize().y) );
            var nTex=new ImageTexture();
            nTex.CreateFromImage(img);
            brushTextures.Add(nTex);
        }
    }


    public override void _Draw(){
        drawHelper.drawPoints(this,pointList,brushTextures,brushIndexList,brushColor);
    }

    public void addPoint(Vector2 pos){
        pointList.Add(pos);
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
