namespace LearnAndAvoid
{

    public class Player
    {
        public int X { get; set; }
        public int Y { get; set; }


        public int Width = 150;
        public int Height = 35;


        public Player(int startX, int startY)
        {
            X = startX;
            Y = startY;
        }

        public void MoveLeft(int amount)
        {
            X -= amount;
            if (X < 0)
            {
                X = 0;
            }
        }



        public void MoveRight(int amount, int screenWidth)
        {
            X += amount;

            if (X + Width > screenWidth)
            {
                X = screenWidth - Width;
            }
        }
    }
}