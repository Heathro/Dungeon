namespace Utils
{
    public class LazyValue<T>
    {
        // STATE

        T value;
        bool initialized = false;
        InitializerDelegate initializer;

        public delegate T InitializerDelegate();

        // CONSTRUCT
      
        public LazyValue(InitializerDelegate initializer)
        {
            this.initializer = initializer;
        }

        // PUBLIC
        
        public T Value
        {
            get
            {
                ForceInit();
                return value;
            }
            set
            {
                initialized = true;
                this.value = value;
            }
        }

        public void ForceInit()
        {
            if (!initialized)
            {
                value = initializer();
                initialized = true;
            }
        }
    }
}