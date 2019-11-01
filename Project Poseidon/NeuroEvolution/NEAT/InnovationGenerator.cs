public static class InnovationGenerator
{
    private static int innovation = 0;
    /// <summary>
    /// Returns a new innovation number
    /// </summary>
    /// <returns></returns>
    public static int GetInnovation()
    {
        innovation++;
        return innovation;
    }
    public static void SetInnovation(int inno)
    {
        if(inno>innovation)
        {
            innovation = inno;
        }
    }
}