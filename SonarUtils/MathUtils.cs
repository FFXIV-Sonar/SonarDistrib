namespace SonarUtils
{
    public static class MathUtils
    {
        public static int AddReturnOriginal(ref int value, int add)
        {
            var ret = value;
            value += add;
            return ret;
        }

        public static int SubstractReturnOriginal(ref int value, int add)
        {
            var ret = value;
            value -= add;
            return ret;
        }
    }
}
