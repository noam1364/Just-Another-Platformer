using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static CurrentLevel;

public class VerticallyMovingPlaform : MovingPlatform
{
    #region ctor
    public VerticallyMovingPlaform(int x,int[]range, bool specialCo=false,plSize size = plSize.Medium) 
        :base(0,range,specialCo,size)
    {
        v = new Vector2(0, vValue);
        if(range[0] > range[1]) range = new int[] { range[1], range[0] };   ///if the range was inputed reverse order,correct it
        ///set Real Position - pos given to Platform and MovingPlatform ctors is temporary
        setPosYCenter((range[0]+range[1])/2);
        setPosLeft(x);
    }
    #endregion ctor
    #region logic
    protected override void movementManager()
    {
        if(Y() <= range[0] || YBottom() >= range[1])
        {
            flipMovement();
        }
        addPosY((int)v.Y);
    }
    #endregion logic
    
}
