using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetMessage
{
    public OpCode code;

    public static void DeserializeMessage(OpCode code, ref DataStreamReader reader)
    {
        switch (code)
        {
            case OpCode.OBJ_POS:
                var o = new Net_ObjectPosition();
                o.Deserialize(ref reader);
                break;
            case OpCode.SCORE:
                var s = new Net_Score();
                s.Deserialize(ref reader);
                break;
            case OpCode.GAME_STATE:
                var g = new Net_GameState();
                g.Deserialize(ref reader);
                break;
            case OpCode.BALL_COLOR:
                var b = new Net_BallColor();
                b.Deserialize(ref reader);
                break;
            case OpCode.START_EFFECT:
                var e = new Net_StartEffect();
                e.Deserialize(ref reader);
                break;

        }
    }

    public virtual void Serialize(ref DataStreamWriter writer)
    {

    }

    public virtual void Deserialize(ref DataStreamReader reader) { 
    
    }
}

//INHERITANCE
public class Net_ObjectPosition : NetMessage
{
    float posX;
    float posY;
    float dirX;
    float dirY;
    int target;

    public Net_ObjectPosition()
    {
        code = OpCode.OBJ_POS;
    }

    public Net_ObjectPosition(float posX, float posY, int target = 0, float dirX = 0, float dirY = 0)
    {
        code = OpCode.OBJ_POS;
        this.posX = posX;
        this.posY = posY;
        this.target = target;
        this.dirX = dirX;
        this.dirY = dirY;
    }
	
	//POLYMORPHISM
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteFloat(posX);
        writer.WriteFloat(posY);
        writer.WriteInt(target);
        writer.WriteFloat(dirX);
        writer.WriteFloat(dirY);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        Vector2 pos = new(reader.ReadFloat(), reader.ReadFloat());
        int t = reader.ReadInt();
        Vector2 dir = new(reader.ReadFloat(),reader.ReadFloat());
        GameManager.Current.ForcePosition(t, pos, dir);
    }
}

public class Net_GameState : NetMessage
{
    int active;

    public Net_GameState()
    {
        code = OpCode.GAME_STATE;
    }

    public Net_GameState(int active)
    {
        code = OpCode.GAME_STATE;
        this.active = active;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteInt(active);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        int active = reader.ReadInt();
        GameManager.Current.SetState(active);
    }
}

public class Net_Score : NetMessage
{
    string score;

    public Net_Score()
    {
        code = OpCode.SCORE;
    }

    public Net_Score(string score)
    {
        code = OpCode.SCORE;
        this.score = score;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteFixedString32(score);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        string score = reader.ReadFixedString32().ToString();
        GameManager.Current.ForceUpdateScore(score);
    }
}

public class Net_BallColor : NetMessage
{
    float r;
    float g;
    float b;
    float a;

    public Net_BallColor()
    {
        code = OpCode.BALL_COLOR;
    }

    public Net_BallColor(float r, float g, float b, float a)
    {
        code = OpCode.BALL_COLOR;
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteFloat(r);
        writer.WriteFloat(g);        
        writer.WriteFloat(b);
        writer.WriteFloat(a);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        Color c = new Color(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        GameManager.Current.ForceBallColor(c);
    }
}

public class Net_StartEffect : NetMessage
{
    string number;
    int active;
    public Net_StartEffect()
    {
        code = OpCode.START_EFFECT;
    }

    public Net_StartEffect(string number, int active = 0)
    {
        code = OpCode.START_EFFECT;
        this.number = number;
        this.active = active;
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteFixedString32(number);
        writer.WriteInt(active);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        string number = reader.ReadFixedString32().ToString();
        int active = reader.ReadInt();
        GameManager.Current.ForceUpdateStartText(number, active);
    }
}

public enum OpCode
{
    OBJ_POS = 1,
    SCORE = 2,
    START_EFFECT = 3,
    GAME_STATE = 4,
    BALL_COLOR = 5
}
