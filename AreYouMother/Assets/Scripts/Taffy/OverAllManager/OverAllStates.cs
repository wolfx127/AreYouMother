namespace Taffy.OverAllManager
{
    public static class OverAllStates
    {
        public static bool isInPlay = false;
        public static bool isInHome = false;
        
        public static bool isInDealer = false;

        public static bool isOpenBag = false;
        public static bool isOpencontainer = false;

        public static void ChangeToPlay()
        {
            isInHome = false;
            isInDealer = false;
            isOpenBag = false;
            isOpencontainer = false;
            isInPlay = true;
        }
        public static void ChangeToHome()
        {
            isInPlay = false;
            isInDealer = false;
            isOpenBag = false;
            isOpencontainer = false;
            isInHome = true;
        }
        public static void ChangeToDealer()
        {
            isInPlay = false;
            isInHome = false;
            isOpenBag = false;
            isOpencontainer = false;
            isInDealer = true;
        }
        public static void ChangeToOpenBag()
        {
            isInPlay = false;
            isInHome = false;
            isInDealer = false;
            isOpencontainer = false;
            isOpenBag = true;
        }
        public static void ChangeToOpenContainer()
        {
            isInPlay = false;
            isInHome = false;
            isInDealer = false;
            isOpenBag = false;
            isOpencontainer = true;
        }
    }
}
