namespace Taffy.OverAllManager
{
    public static class OverAllStates
    {
        public static bool isInPlay = false;
        public static bool isInHome = false;
        
        public static bool isInWarehouse = false;
        public static bool isInDealer = false;

        public static bool isOpenBag_A = false;
        public static bool isOpenContainer_A = false;
        public static bool isOpenBag_B = false;
        public static bool isOpenContainer_B = false;

        public static void ChangeToPlay()
        {
            isInHome = false;
            isInDealer = false;
            isInWarehouse = false;
            
            isOpenBag_A = false;
            isOpenContainer_A = false;
            isOpenBag_B = false;
            isOpenContainer_B = false;
            
            isInPlay = true;
        }
        public static void ChangeToHome()
        {
            isInPlay = false;
            
            isInDealer = false;
            
            isOpenBag_A = false;
            isOpenContainer_A = false;
            isOpenBag_B = false;
            isOpenContainer_B = false;
            
            isInHome = true;
            isInWarehouse = true;
        }

        public static void ChangeToWarehouse()
        {
            if (!isInHome) return;
            isInDealer = false;
            
            isOpenBag_A = false;
            isOpenContainer_A = false;
            isOpenBag_B = false;
            isOpenContainer_B = false;
            
            isInWarehouse = true;
        }
        public static void ChangeToDealer()
        {
            if (!isInHome) return;
            isInWarehouse = false;
            
            isOpenBag_A = false;
            isOpenContainer_A = false;
            isOpenBag_B = false;
            isOpenContainer_B = false;
            
            isInDealer = true;
        }
        public static void ChangeToOpenBag_A()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;
            
            isOpenContainer_A = false;
            
            isOpenBag_A = true;
        }
        public static void ChangeToOpenContainer_A()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;
            
            isOpenBag_A = true;
            isOpenContainer_A = true;
        }
        public static void ChangeToCloseBagAndContainer_A()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;
            
            isOpenContainer_A = false;
            isOpenBag_A = false;
        }
        public static void ChangeToOpenBag_B()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;

            isOpenContainer_B = false;

            isOpenBag_B = true;
        }
        public static void ChangeToOpenContainer_B()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;

            isOpenBag_B = true;
            isOpenContainer_B = true;
        }
        public static void ChangeToCloseBagAndContainer_B()
        {
            if (!isInPlay) return;
            isInHome = false;
            isInWarehouse = false;
            isInDealer = false;

            isOpenContainer_B = false;
            isOpenBag_B = false;
        }
    }
}
