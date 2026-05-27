namespace LearnAndAvoid;

public class Obstacle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 30;
    public int Height { get; set; } = 30;

    public int Speed { get; set; }

    // Eğer true ise nesne bomba olur, false ise C# kodu olur
    public bool IsBomb { get; set; }

    // Ekranda görünecek olan C# kelimesini tutan özellik
    public string SymbolText { get; set; } = "";

    // Yeni nesne oluşturulurken çalışan yapıcı metot (Constructor)
    public Obstacle(int startX, int startY, int speed, bool isBomb)
    {
        X = startX;
        Y = startY;
        Speed = speed;
        IsBomb = isBomb;
    }

    // Nesnenin aşağı doğru düşmesini sağlayan metot
    public void MoveDown()
    {
        Y += Speed;
    }
}