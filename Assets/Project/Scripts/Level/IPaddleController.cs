namespace Twode.Pong
{
    public interface IPaddleController
    {
        public Paddle Paddle { get; set; }
        
        public void SetInputEnabled(bool inputEnabled);
    }
}