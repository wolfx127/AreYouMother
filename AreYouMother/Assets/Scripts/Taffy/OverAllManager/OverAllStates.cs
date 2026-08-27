namespace Taffy.OverAllManager
{
    public static class OverAllStates
    {
        public static bool isInPlay = false;
        public static bool isInHome = false;
        
        public static bool isInDealer = false;

        public static void ChangeToPlay()
        {
            isInHome = false;
            isInDealer = false;
            isInPlay = true;
        }
        public static void ChangeToHome()
        {
            isInPlay = false;
            isInDealer = false;
            isInHome = true;
        }
        public static void ChangeToDealer()
        {
            isInPlay = false;
            isInHome = false;
            isInDealer = true;
        }
    }
}
