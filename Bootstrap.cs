namespace Kobanan
{
    public class Bootstrap
    {
        public IWorld _w;
        public void Main()
        {
            _w = new World();
        }

        public void Update()
        {
            _w.Update();
        }

        public void Destroy()
        {
            _w.Destroy();
        }
    }
}