namespace MackySoft.XPool.Collections.Internal
{
    public static partial class NoReferenceTypeRegistry {
        
        public static void Register<T>() {
            Holder<T>.IsRegistered = true;
        }
        
        internal static bool IsRegistered<T>() => Holder<T>.IsRegistered;
        static class Holder<T> {
            public static bool IsRegistered;
        }
    }
    
}