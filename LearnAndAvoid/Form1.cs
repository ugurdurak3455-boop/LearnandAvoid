namespace LearnAndAvoid;

public partial class Form1 : Form
{
    private Player player;
    private System.Windows.Forms.Timer gameTimer;

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true;
        this.ClientSize = new Size(900, 400);
        this.BackColor = Color.Black;

        player = new Player(420, 350);

        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = 16;
        gameTimer.Tick += GameLoop;
        gameTimer.Start();

        this.KeyDown += OnKeyDown;
    }

    private void GameLoop(object sender, EventArgs e)
    {
        this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.FillRectangle(Brushes.CornflowerBlue,
            player.X, player.Y, player.Width, player.Height);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Left)
            player.MoveLeft(10);
        if (e.KeyCode == Keys.Right)
            player.MoveRight(10);
        this.Invalidate();

    }
}