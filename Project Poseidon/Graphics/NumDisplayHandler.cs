using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public static class NumDisplayHandler
{
    public static Texture2D digitsTex = AssetManager.getSprite("Digits")[0];
 
    private static List<int> intToList(int num)
    {
        char[] arr = num.ToString().ToCharArray();
        List<int> digit = new List<int>();
        for (int i = 0; i < arr.Length; i++)
            digit.Add(arr[i] - '0');
        return digit;
    }
    public static void draw(Point pos,int n,int digNum = 0)
    {///digNum--> a flag that says how many digits should be drawan
        List<int> numLst = intToList(n);
        if(numLst.Count<digNum&&digNum>0)
        {
            List<int> zeros = new List<int>();
            for(int i = 0;i < digNum - numLst.Count;i++)
                zeros.Add(0);
            zeros.AddRange(numLst);
            numLst = zeros;
        }

        int dx = digitsTex.Width / 10;
        int[] num = numLst.ToArray();
        if(digNum == 0) digNum = num.Length;
        for (int i = 0; i < digNum; i++)
            Global.sb.Draw(digitsTex, new Vector2(pos.X+dx*(num.Length-i-1), pos.Y), new Rectangle(dx * num[num.Length - 1 - i], 0, digitsTex.Width / 10, digitsTex.Height), Color.White);

    }
}


