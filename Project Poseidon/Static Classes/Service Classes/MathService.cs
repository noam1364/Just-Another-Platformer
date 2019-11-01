using static System.Math;


public static class MathService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">The coefficient of x^2</param>
    /// <param name="b">The coefficient of x</param>
    /// <param name="c">The free term</param>
    /// <returns></returns>
	public static float[] solveQuadEq(float a,float b,float c)
    {///Quadratic formula : (-b+-sqrt(b^2-4*a*c))\2a
        double disc = Pow(b, 2) - 4 * a * c;
        if(disc < 0) return null;
        return new float[] {(float)((-b+Sqrt(disc))/(2*a)),(float)((-b-Sqrt(disc))/(2*a))};
    }
    /// <summary>
    /// returns the index of the array elemnts with the heighest value
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static int getMaxIdx(float[] arr)
    {
        int maxIdx = 0;
        for(int i = 0;i< arr.Length;i++)
        {
            if(arr[i] > arr[maxIdx])
                maxIdx = i;
        }
        return maxIdx;
    }
    /// <summary>
    /// Recives a vector and returns a vector of the same size, where the values add up to 1 and
    /// the values are matching the input, by size
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static float[] softmax(float[] arr)
    {
        float[] expArr = new float[arr.Length],result = new float[arr.Length];
        float expSum = 0;
        for(int i = 0;i < arr.Length;i++)
        {
            expArr[i] = (float)System.Math.Exp(arr[i]);
            expSum += expArr[i];
        }
        for(int i=0;i<result.Length;i++)
        {
            result[i] = expArr[i] / expSum;
        }
        return result;
    }
    public static float sigmoid(float x)
    {
        float exp = (float)System.Math.Exp(-4.9*x);
        return exp / (exp + 1);
    }
    /// <summary>
    /// return a random integer that equals to 'from' or higher,and lower then 'to'
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static int randomInt(int from,int to)
    {
        return (int)(Global.random.NextDouble()*(to-from)+from);
    }
}
