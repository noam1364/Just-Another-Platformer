using System;

namespace Project_Poseidon
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new JustAnotherPlatformer())
                game.Run();
        }
    }
#endif
}
