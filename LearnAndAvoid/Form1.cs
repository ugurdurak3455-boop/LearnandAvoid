using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LearnAndAvoid
{
    public partial class Form1 : Form
    {
        private Player player;
        private System.Windows.Forms.Timer gameTimer;
        private List<Obstacle> obstacles = new List<Obstacle>();
        private Random rnd = new Random();

        private int lives = 3;
        private int score = 0;
        private bool isPaused = false; // Oyunun duraklatılma durumunu tutar
        private bool isGameOver = false;
        private int baseSpeed = 3;       // Oyunun en başındaki düşme hızı
        private int currentSpeed = 3;    // Anlık hız (Skor arttıkça artacak)
        private int spawnTimer = 0;      // Nesnelerin geliş süresini sayacak sayaç
        private int spawnInterval = 50;  // İki nesne arasındaki süre aralığı

        // --- GÜNCELLENEN DEĞİŞKENLER (İLERİYE SAYAN SÜRE SİSTEMİ) ---
        private int elapsedTime = 0; // 0'dan başlayıp ileriye doğru sayacak süre (saniye)
        private System.Windows.Forms.Timer stopwatchTimer; // Zamanı ileri saydıracak zamanlayıcı
        private Label lblHearts; // Kalp emojilerini gösterecek metin alanı
        private Label lblCountdown; // Geçen süreyi ekranın sağ üstünde gösterecek metin alanı

        // Oyunda yukarıdan düşecek C# kelime havuzu
        private string[] csharpConcepts = {
            "int", "string", "bool", "float", "double", "char", "byte", "long", "short", "decimal", "uint", "object", "void", "var", "dynamic",
            "public", "private", "protected", "internal", "sealed", "abstract", "static", "readonly", "const", "virtual", "override",
            "if", "else", "switch", "case", "for", "foreach", "while", "do", "break", "continue", "return", "yield",
            "class", "struct", "interface", "enum", "record", "delegate", "event", "namespace", "using", "base", "this",
            "try", "catch", "finally", "throw", "Exception", "when",
            "=>", "from", "where", "select", "orderby", "groupby", "join", "ascending", "descending",
            "async", "await", "Task", "Task<T>", "ValueTask", "CancellationToken",
            "??", "?.", "??=", "is", "as", "typeof", "sizeof", "nameof", "ref", "out", "in"
        };

        // Her saniye süreyi 1 artıran (kronometre) metot
        private void StopwatchTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameOver) return;
            if (isPaused) return; // Duraklatıldığında zamanın akmasını engeller

            elapsedTime++;

            int minutes = elapsedTime / 60;
            int seconds = elapsedTime % 60;

            lblCountdown.Text = "Time: " + minutes + ":" + seconds.ToString("D2");
        }

        // Bombaya her çarptığında canı azaltan ve kalpleri güncelleyen metot
        private void DecreaseLives()
        {
            lives--; // Her çarpmada canı 1 azalt

            // Canın durumuna göre kalpleri güncelle
            if (lives == 2)
            {
                lblHearts.Text = "Can: ❤️❤️"; // 3. kalp gizlenir, oyun sürer
            }
            else if (lives == 1)
            {
                lblHearts.Text = "Can: ❤️"; // 2. kalp gizlenir, oyun sürer
            }
            else if (lives <= 0)
            {
                lblHearts.Text = "Can: "; // Kalp kalmadı, oyun biter
                EndGame();
            }
        }

        // Oyun bittiğinde çalışan metot
        private void EndGame()
        {
            isGameOver = true;
            gameTimer.Stop();
            stopwatchTimer.Stop();

            int minutes = elapsedTime / 60;
            int seconds = elapsedTime % 60;
            string finalTime = minutes + ":" + seconds.ToString("D2");

            
            MessageBox.Show($"Game Over!\nTotal Score: {score}\nSurvival Time: {finalTime}\n\nPress 'R' to Restart the Game!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public Form1()
        {
            InitializeComponent();

            this.BackgroundImage = Properties.Resources.gameBackground;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.DoubleBuffered = true;

            int startingY = this.ClientSize.Height - 80;
            player = new Player(420, startingY);

            // --- GÖRSEL CAN SİSTEMİ BAŞLANGIÇ AYARLARI ---
            lblHearts = new Label();
            lblHearts.Text = "Can: ❤️❤️❤️";
            lblHearts.Font = new Font("Arial", 14, FontStyle.Bold);
            lblHearts.ForeColor = Color.White;
            lblHearts.AutoSize = true;
            lblHearts.Location = new Point(10, 40); // Skor yazısının hemen altına yerleşir
            lblHearts.BackColor = Color.Transparent;
            this.Controls.Add(lblHearts);

            // --- GÖRSEL SÜRE SAYACI BAŞLANGIÇ AYARLARI (SAĞ ÜST KÖŞE) ---
            lblCountdown = new Label();
            lblCountdown.Text = "Time: 0:00";
            lblCountdown.Font = new Font("Arial", 14, FontStyle.Bold);
            lblCountdown.ForeColor = Color.White;
            lblCountdown.AutoSize = true;
            lblCountdown.Location = new Point(this.ClientSize.Width - 140, 10); // Sağ üst köşe konumu
            lblCountdown.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Ekran boyutu değişse de sağda kalır
            lblCountdown.BackColor = Color.Transparent;
            this.Controls.Add(lblCountdown);

            // --- SÜRE ZAMANLAYICISI (KRONOMETRE) AYARLARI ---
            stopwatchTimer = new System.Windows.Forms.Timer();
            stopwatchTimer.Interval = 1000; // 1 saniyede bir tetiklenir
            stopwatchTimer.Tick += StopwatchTimer_Tick;
            stopwatchTimer.Start();

            // --- OYUN DÖNGÜSÜ (GAME LOOP) TIMER AYARLARI ---
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown += OnKeyDown;
            this.Resize += Form1_Resize;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (isGameOver) return;
            if (isPaused) return; // Eğer oyun duraklatıldıysa döngüden çık, hiçbir şeyi oynatma

            // --- DİNAMİK ZORLUK SEVİYESİ (DÜZELTİLDİ) ---
            currentSpeed = baseSpeed + (score / 200);

            // %7 ihtimalle yeni bir nesne üret
            if (rnd.Next(0, 100) < 7)
            {
                int randomX = rnd.Next(0, this.ClientSize.Width - 30);

                // HIZ DÜZELTİLDİ: Artık currentSpeed değişkenine bağlı olarak nesneler hızlanacak
                int randomSpeed = rnd.Next(currentSpeed, currentSpeed + 4);
                bool spawnAsBomb = rnd.Next(0, 100) < 40;

                // Yeni düşen nesneyi oluşturuyoruz
                Obstacle newObstacle = new Obstacle(randomX, 0, randomSpeed, spawnAsBomb);

                // Eğer nesne bomba değilse, havuzdan rastgele bir kod seç ve ona ata
                if (!spawnAsBomb)
                {
                    int randomIndex = rnd.Next(csharpConcepts.Length);
                    newObstacle.SymbolText = csharpConcepts[randomIndex];

                    // Çarpışma kutusunu kelimenin uzunluğuna göre büyüt
                    newObstacle.Width = newObstacle.SymbolText.Length * 11;
                }

                obstacles.Add(newObstacle);
            }

            // Nesneleri hareket ettir ve çarpışmaları kontrol et
            for (int i = obstacles.Count - 1; i >= 0; i--)
            {
                obstacles[i].MoveDown();

                Rectangle playerRect = new Rectangle(player.X, player.Y, player.Width, player.Height);
                Rectangle obstacleRect = new Rectangle(obstacles[i].X, obstacles[i].Y, obstacles[i].Width, obstacles[i].Height);

                // Oyuncu yukarıdan düşen bir nesneye çarptığında:
                if (playerRect.IntersectsWith(obstacleRect))
                {
                    if (obstacles[i].IsBomb)
                    {
                        // Bombaya çarptıysa can kaybeder ve bomba ekrandan silinir
                        DecreaseLives();
                        obstacles.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        // C# koduna çarptıysa puan kazanır ve nesne ekrandan silinir
                        score += 10;
                        obstacles.RemoveAt(i);
                        continue;
                    }
                }

                // Nesne oyuncuya çarpmadan ekranın altından çıkıp giderse:
                if (obstacles[i].Y > this.ClientSize.Height)
                {
                    // Bombalardan başarıyla kaçtığı için küçük bir ödül puanı
                    if (obstacles[i].IsBomb)
                    {
                        score += 5;
                    }
                    obstacles.RemoveAt(i);
                }
            }

            this.Invalidate(); // Ekranı yenile
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Oyuncuyu sarı dikdörtgen olarak çiz
            g.FillRectangle(Brushes.Yellow, player.X, player.Y, player.Width, player.Height);

            // Ekrandaki tüm nesneleri türüne göre çiz
            foreach (var obstacle in obstacles)
            {
                if (obstacle.IsBomb)
                {
                    // Bombaysa bomba resmini çiz
                    g.DrawImage(Properties.Resources.bombaResmi, obstacle.X, obstacle.Y, obstacle.Width, obstacle.Height);
                }
                else
                {
                    Font codeFont = new Font("Consolas", 18, FontStyle.Bold);
                    int offset = 2; // Çerçeve kalınlığı

                    // 4 tarafa siyah gölge çizerek tam bir dış çerçeve (outline) oluşturuyoruz
                    g.DrawString(obstacle.SymbolText, codeFont, Brushes.Black, obstacle.X - offset, obstacle.Y);
                    g.DrawString(obstacle.SymbolText, codeFont, Brushes.Black, obstacle.X + offset, obstacle.Y);
                    g.DrawString(obstacle.SymbolText, codeFont, Brushes.Black, obstacle.X, obstacle.Y - offset);
                    g.DrawString(obstacle.SymbolText, codeFont, Brushes.Black, obstacle.X, obstacle.Y + offset);

                    // Ana metni Yeşil-Sarı yaparak parlamasını sağlıyoruz
                    g.DrawString(obstacle.SymbolText, codeFont, Brushes.GreenYellow, obstacle.X, obstacle.Y);
                }
            }

            // Sol üst köşeye skoru çiz
            g.DrawString($"Score: {score}", new Font("Arial", 14, FontStyle.Bold), Brushes.White, 10, 10);

            // ============================================================
            // 4. ADIM BURAYA EKLENDİ (PAUSED EKRAN PERDESİ VE MOR YAZI)
            // ============================================================
            if (isPaused)
            {
                // Arkaya yarı saydam siyah bir katman çekerek oyunu karartıyoruz
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, this.ClientSize.Width, this.ClientSize.Height);

                Font pauseFont = new Font("Impact", 36, FontStyle.Bold);
                string pauseText = "GAME PAUSED\nPress 'P' to Resume";

                // Yazıyı ekranın tam ortasına hizalamak için ölçüyoruz
                Size textSize = TextRenderer.MeasureText(pauseText, pauseFont);
                int posX = (this.ClientSize.Width - textSize.Width) / 2;
                int posY = (this.ClientSize.Height - textSize.Height) / 2;

                // Mor temaya uygun şekilde "GAME PAUSED" metnini çiziyoruz
                g.DrawString(pauseText, pauseFont, Brushes.MediumOrchid, posX, posY);
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // --- YENİ OYUN / YENİDEN BAŞLATMA (RESTART) SİSTEMİ ---
            if (e.KeyCode == Keys.R && isGameOver)
            {
                lives = 3;
                score = 0;
                elapsedTime = 0;
                currentSpeed = baseSpeed;
                isGameOver = false;
                isPaused = false;

                obstacles.Clear();

                lblHearts.Text = "Can: ❤️❤️❤️";
                lblCountdown.Text = "Time: 0:00";

                gameTimer.Start();
                stopwatchTimer.Start();

                this.Invalidate();
                return;
            }

            // --- DURAKLATMA (PAUSE) SİSTEMİ --- 
            if (e.KeyCode == Keys.P && !isGameOver)
            {
                isPaused = !isPaused;

                if (isPaused)
                {
                    stopwatchTimer.Stop();
                    this.Invalidate();
                }
                else
                {
                    stopwatchTimer.Start();
                    this.Invalidate();
                }
            }

            if (isPaused) return;

            if (e.KeyCode == Keys.Left)
                player.MoveLeft(20);
            if (e.KeyCode == Keys.Right)
                player.MoveRight(20, this.ClientSize.Width);
     
        }



        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (player != null)
            {
                player.Y = this.ClientSize.Height - 60;
            }
        }
    }
}