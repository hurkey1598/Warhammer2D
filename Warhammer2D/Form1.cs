using System.Numerics;
using System.Runtime.InteropServices;

namespace Warhammer2D
{
    public partial class Form1 : Form
    {
        // Enum for different states of the game
        public enum GameState
        {
            Setup,
            PlayerMove,
            PlayerShoot,
            EnemyMove,
            EnemyShoot
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.Click += Form_Click;
            this.Paint += GridLines;
            this.MouseMove += MouseMoved;

            // Initialize setup and buttons
            SetupEndButton();
            EndTurnButton();
            ShootButton();
            NextPhaseButton();
            SetupDiceBox();

            // Place initial mountains and Necrons on the board
            PlaceMountains(12);
            PlaceNecrons(4 + level); // maximum 27 or game has risk of crashing crashes

            TurnDisplay();

        }

        // Player and enemy pieces on the board
        public List<Character> playerChars = new List<Character>();
        public List<Character> enemyChars = new List<Character>();
        private HashSet<Point> mountainPositions = new HashSet<Point>();
        public Character? playerSelected;
        public GameState currentState = GameState.Setup;
        private int piecesToPlace = 5;

        // Random number generator for generating positions for enemies start
        private Random random = new Random();

        // Keeps track of how many units there are on both sides
        public int spaceMarineCount = 0;
        public int necronCount = 0;

        // Variables for grid and UI layout
        private int MX = 10;
        private int MY = 10;
        private int squaresize = 50;
        private int highwidth = 15;
        private Rectangle[] boxes;
        private int level = 0;

        // Items on screen
        private Button SetupendBtn;
        private Button TurnendBtn;
        private TextBox TurnDisplayBox;
        private Button ShootBtn;
        private Button nextPhaseBtn;

        public Character? shooter;
        public Character? target;
        public int score = 0;
        public int dice = 0;
        private TextBox DiceBox;


        public Form1()
        {
            InitializeComponent();
                      
        }

        // Creates and configures the "End Setup" button
        private void SetupEndButton()
        {
            SetupendBtn = new Button
            {
                Text = "End Setup",
                Size = new Size(100, 30),
                Location = new Point(highwidth * squaresize + 20, 10),
                Enabled = false,
                BackColor = Color.Transparent
            };
            SetupendBtn.Click += SetupEndBtn_Click;
            this.Controls.Add(SetupendBtn);
        }

        // Creates and configures the "End Turn" button
        private void EndTurnButton()
        {
            TurnendBtn = new Button
            {
                Text = "End Turn",
                Size = new Size(100, 30),
                Location = new Point(highwidth * squaresize + 20, 10),
                Enabled = false,
                Visible = false,
                BackColor = Color.Transparent
            };
            TurnendBtn.Click += EndTurnBtn_Click;
            this.Controls.Add(TurnendBtn);
        }

        private void TurnDisplay()
        {
            TurnDisplayBox = new TextBox
            {
                Text = currentState.ToString(),
                Size = new Size(100, 50),
                Location = new Point(highwidth * squaresize + 20, 230),
                TextAlign = HorizontalAlignment.Center,
                Enabled = false
            };
            this.Controls.Add(TurnDisplayBox);
        }

        // "End Setup" button click
        private void SetupEndBtn_Click(object sender, EventArgs e)
        {
            while (spaceMarineCount <= 3)
            {
                DialogResult dialogResult = MessageBox.Show("Place more??", "Units remaining", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    return;
                }

                if (dialogResult == DialogResult.No)
                {
                    currentState = GameState.EnemyMove; // Transition to EnemyMove state
                    SetupendBtn.Text = "Game Active";
                    SetupendBtn.Visible = false;
                    break;
                }
            }
            currentState = GameState.EnemyMove; // Transition to EnemyMove state
            MoveNecrons();
            SetupendBtn.Text = "Game Active";
            SetupendBtn.Visible = false;
            TurnendBtn.Visible = true;
            TurnendBtn.Enabled = true;

            TurnDisplayBox.Text = currentState.ToString();
            nextPhaseBtn.Visible = true;
        }

        // "End Turn" button click
        private void EndTurnBtn_Click(object sender, EventArgs e)
        {
            // Switch to enemy move phase
            currentState = GameState.EnemyMove;
            MoveNecrons();

            // Switch to enemy shoot phase
            currentState = GameState.EnemyShoot;
            EnemyShoot();

            // Goes back to PlayerMove phase
            TurnendBtn.Enabled = true;
            ResetHumanMove();
            ResetHumanShoot();
            currentState = GameState.PlayerMove;
            TurnDisplayBox.Text = currentState.ToString();
            
        }

        private void MoveNecrons()
        {
            HashSet<Point> usedPositions = new HashSet<Point>(mountainPositions);
            foreach (Character c in enemyChars)
            {
                usedPositions.Add(c.image.Location);
            }
            foreach (Character c in playerChars)
            {
                usedPositions.Add(c.image.Location);
            }
            foreach (Character c in enemyChars)
            {
                c.CPUmove(usedPositions, squaresize);
            }
            currentState = GameState.EnemyShoot;

            EnemyShoot();
            ResetHumanShoot();
        }

        public void clearSelected(bool isThePlayer)
        {
            if (isThePlayer) 
            { 
                foreach (Character c in playerChars)
                {
                    c.isSelected = false;
                    c.image.BorderStyle = BorderStyle.None;
                }
            }
            else
            {
                foreach (Character c in enemyChars)
                {
                    c.isSelected = false;
                    c.image.BorderStyle = BorderStyle.None;
                }
            }

        }


        // Click events on the form during setup or player movement
        private void Form_Click(object sender, EventArgs e)
        {
            if (currentState == GameState.Setup)
            {
                // Allow placing players only in the last two rows
                if (MY >= ((highwidth - 2) * squaresize) && MX < 750 && MY < 750)
                {
                    piecesToPlace--;
                    // Enable the "End Setup" button once all pieces are placed
                    if (spaceMarineCount >= 0)
                    {
                        SetupendBtn.Enabled = true;
                        

                    }
                    // Add the player piece to the list and dictionary and adds them to a count
                    spaceMarineCount++; // Increment the count for Space Marines
                    Character plyr = new Character(MX, MY, Properties.Resources.Space_marine_Background_Removed, squaresize, true, this);
                    playerChars.Add(plyr);
                    if (piecesToPlace <= 0)
                    {
                        currentState = GameState.EnemyMove;
                        MoveNecrons();
                        SetupendBtn.Visible = false;
                        TurnendBtn.Visible = true;
                        TurnendBtn.Enabled = true;
                        nextPhaseBtn.Visible = true;
                    }
                }
                else
                {
                    MessageBox.Show("You can only place players in the last two rows.", "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (currentState == GameState.PlayerMove)
            {
                // Check if a player character is selected
                if (playerSelected == null)
                {
                    // Select player character
                    playerSelected = playerChars.FirstOrDefault(c => c.image.Bounds.Contains(MX, MY));
                    if (playerSelected != null)
                    {
                        clearSelected(true);
                        playerSelected.isSelected = true;
                        playerSelected.image.BorderStyle = BorderStyle.FixedSingle;
                        ShootBtn.Enabled = true; // Enable the shoot button when a player character is selected
                    }
                }
                else
                {
                    // Calculate the movement distance and check restrictions
                    int dx = Math.Abs(playerSelected.image.Location.X - MX) / squaresize;
                    int dy = Math.Abs(playerSelected.image.Location.Y - MY) / squaresize;
                    int distance = dx + dy;

                    if (playerSelected.hasMoved)
                    {
                        MessageBox.Show("This unit has already moved this turn", "Move Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (distance > 5)
                    {
                        MessageBox.Show($"This unit cannot move more than 5 squares!", "Move Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    int playersMoved = 0;
                    foreach (Character c in playerChars)
                    {
                        if (MX <= 750 && MY <= 750)
                        {
                            if (c == playerSelected)
                            {
                                c.hasMoved = true;
                                c.image.Location = new Point(MX, MY);
                                clearSelected(true);
                            }
                            if (c.hasMoved)
                            {
                                playersMoved++;
                                if (playersMoved == spaceMarineCount)
                                {

                                    // Goes back to PlayerMove phase
                                    TurnendBtn.Enabled = true;
                                    currentState = GameState.PlayerShoot;
                                    ShootBtn.Enabled = true;
                                    TurnDisplayBox.Text = currentState.ToString();
                                    ResetHumanMove();
                                    return;
                                }
                            }
                        }
                        
                    }
                    playerSelected = null;
                }
            }
            else if (currentState == GameState.PlayerShoot)
            {
                // Select player character if not already selected
                if (playerSelected == null)
                {
                    playerSelected = playerChars.FirstOrDefault(c => c.image.Bounds.Contains(MX, MY));
                    if (playerSelected != null)
                    {
                        clearSelected(true);
                        playerSelected.isSelected = true;
                        playerSelected.image.BorderStyle = BorderStyle.FixedSingle;
                        ShootBtn.Enabled = true; // Enable the shoot button when a player character is selected
                    }
                }
                else
                {
                    // Select enemy character
                    target = enemyChars.FirstOrDefault(c => c.image.Bounds.Contains(MX, MY));
                    if (target != null)
                    {
                        target.isSelected = true;
                        target.image.BorderStyle = BorderStyle.FixedSingle;
                        ShootBtn.Enabled = true; // Enable the shoot button when an enemy character is selected
                    }
                }
            }
            TurnDisplayBox.Text = currentState.ToString();
            this.Invalidate();
        }

        // Draws the grid and highlights things of relevance
        private void GridLines(object sender, PaintEventArgs e)
        {
            // Draw grid lines
            for (int i = 0; i < highwidth + 1; i++)
            {
                e.Graphics.DrawLine(Pens.Red, i * squaresize, 0, i * squaresize, squaresize * highwidth);
                e.Graphics.DrawLine(Pens.Red, 0, i * squaresize, squaresize * highwidth, i * squaresize);
            }

            if ( MX < 750 && MY < 750)
            {
                e.Graphics.DrawRectangle(Pens.Green, MX, MY, squaresize, squaresize);
            }
            
        }

        // Updates mouse position on the grid
        private void MouseMoved(object sender, MouseEventArgs e)
        {
            MX = 1 + Convert.ToInt32(e.X / squaresize) * squaresize;
            MY = 1 + Convert.ToInt32(e.Y / squaresize) * squaresize;
            this.Invalidate(); // Refresh the form to update UI
        }

        // Places a specified number of mountains on the board
        private void PlaceMountains(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Point mountainLocation;
                do
                {
                    // Generate random position for the mountain
                    int randomX = random.Next(0, highwidth) * squaresize + 1;
                    int randomY = random.Next(0, highwidth) * squaresize + 1;
                    mountainLocation = new Point(randomX, randomY);
                } while (mountainPositions.Contains(mountainLocation)); // Makes sure there are no duplicates

                mountainPositions.Add(mountainLocation);
                PictureBox mountain = new PictureBox
                {
                    Size = new Size(squaresize - 2, squaresize - 2),
                    Location = mountainLocation,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Properties.Resources.mountain_pixel_removebg_preview,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(mountain);
            }
        }

        // Places a specified number of Necrons on the board
        private void PlaceNecrons(int count)
        {
            HashSet<Point> usedPositions = new HashSet<Point>(mountainPositions);
            for (int i = 0; i < count; i++)
            {
                Point necronLocation;
                do
                {
                    // Generate random position for the Necrons to move to
                    int randomX = random.Next(0, highwidth) * squaresize + 1;
                    int randomY = random.Next(0, 2) * squaresize + 1;
                    necronLocation = new Point(randomX, randomY);
                } while (usedPositions.Contains(necronLocation)); // Makes sure there are no duplicates

                usedPositions.Add(necronLocation);
                Character nec = new Character(necronLocation.X, necronLocation.Y, Properties.Resources.necron, squaresize, false, this);
                enemyChars.Add(nec);
                necronCount++;
            }
        }

        private void ResetHumanMove()
        {
            foreach (Character c in playerChars)
            {
                c.hasMoved = false;
            }
        }

        //////////// Shooting and Shooting button /////////////

        private void ResetHumanShoot()
        {
            foreach (Character c in playerChars)
            {
                c.hasShot = false;
            }
        }


        private void EnemyShoot()
        {
            // Ensure there are Space Marines to target
            if (playerChars.Count > 0)
            {
                foreach (Character enemy in enemyChars)
                {
                    // Find the closest player character within range
                    Character? target = null;
                    int minDistance = int.MaxValue;

                    foreach (Character player in playerChars)
                    {
                        // Calculate the distance between enemy and player
                        int dx = Math.Abs(enemy.image.Location.X - player.image.Location.X) / squaresize;
                        int dy = Math.Abs(enemy.image.Location.Y - player.image.Location.Y) / squaresize;
                        int distance = dx + dy;

                        // Check if the player is within 5 squares and closer than the current target
                        if (distance <= 5 && distance < minDistance)
                        {
                            target = player;
                            minDistance = distance;
                        }
                    }

                    // If a target is found within range, shoot it
                    if (target != null)
                    {
                        enemy.Shoot(target);

                        // Remove the target from the player list and the form's controls
                        playerChars.Remove(target);
                        this.Controls.Remove(target.image);
                        

                        // Check if all player units are dead
                        if (playerChars.Count == 0)
                        {
                            ShowLoseScreen();
                            return;
                        }

                        
                    }
                }
            }

            currentState = GameState.PlayerMove;
        }

        private void ShootButton()
        {
            ShootBtn = new Button();
            ShootBtn.Text = "Shoot";
            ShootBtn.Size = new Size(100, 30);
            ShootBtn.Location = new Point(highwidth * squaresize + 20, 60);
            ShootBtn.BackColor = Color.Transparent;
            ShootBtn.Click += ShootBtn_Click;
            ShootBtn.Enabled = false;

            this.Controls.Add(ShootBtn);
        }

        private void DiceRoll()
        {
            Random rnd = new Random();
            dice = rnd.Next(1, 7);
            DiceBox.Text = dice.ToString();
        }

        
        private void ShootBtn_Click(object sender, EventArgs e)
        {
            
            if (currentState != GameState.PlayerShoot || playerSelected == null || target == null)
            {
                return;
            }

            DiceRoll();

            if (dice >= 3)
            {
                if (shooter != null && target != null)
                {

                    // Check if the shooter has already shot
                    if (shooter.hasShot)
                    {
                        MessageBox.Show("This unit has already shot this turn", "Shooting Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Calculate the distance between shooter and target
                    int dx = Math.Abs(shooter.image.Location.X - target.image.Location.X) / squaresize;
                    int dy = Math.Abs(shooter.image.Location.Y - target.image.Location.Y) / squaresize;
                    int distance = dx + dy;

                    // Check if the target is within 5 squares
                    if (distance > 5)
                    {
                        MessageBox.Show("Target is out of range! You can only shoot enemies within 5 squares.", "Shooting Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    shooter.Shoot(target);
                    shooter.hasShot = true; // Set hasShot to true after shooting


                    // Remove the target from the enemy list and the form's controls
                    enemyChars.Remove(target);
                    this.Controls.Remove(target.image);

                    // Reset the target
                    target = null;

                    TurnDisplayBox.Text = currentState.ToString();
                    this.Invalidate(); // Refresh the form to update UI
                }
            }
            if (dice < 3) 
            {
                MessageBox.Show("You missed", "Shooting missed", MessageBoxButtons.OK);
                shooter.hasShot = true; // Set hasShot to true after shooting
                target = null;
                TurnDisplayBox.Text = currentState.ToString();

                if (shooter.hasShot)
                {
                    MessageBox.Show("This unit has already shot this turn", "Shooting Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void SetupDiceBox()
        {
            DiceBox = new TextBox();
            DiceBox.Size = new Size(100, 50);
            DiceBox.Location = new Point(highwidth * squaresize + 20, 200);
            DiceBox.TextAlign = HorizontalAlignment.Center;
            DiceBox.Text = dice.ToString();
            DiceBox.Enabled = false;
            this.Controls.Add(DiceBox);
        }

        //////////// Next Phase Button /////////////

        private void NextPhaseButton()
        {
            nextPhaseBtn = new Button();
            nextPhaseBtn.Text = "Next Phase";
            nextPhaseBtn.Size = new Size(100, 30);
            nextPhaseBtn.Location = new Point(highwidth * squaresize + 20, 100);
            nextPhaseBtn.Enabled = true;
            nextPhaseBtn.Visible = false;
            nextPhaseBtn.BackColor = Color.Transparent;
            nextPhaseBtn.Click += NextPhaseBtn_Click;
            this.Controls.Add(nextPhaseBtn);
        }

        private void NextPhaseBtn_Click(object sender, EventArgs e)
        {
            switch (currentState)
            {
                case GameState.Setup:
                    currentState = GameState.PlayerMove;
                    break;
                case GameState.PlayerMove:
                    currentState = GameState.PlayerShoot;
                    ShootBtn.Enabled =true;
                    ResetHumanMove();
                    break;
                case GameState.PlayerShoot:
                    currentState = GameState.EnemyMove;
                    ResetHumanMove();
                    MoveNecrons();
                    break;
                case GameState.EnemyMove:
                    currentState = GameState.EnemyShoot;
                    EnemyShoot();
                    break;
                case GameState.EnemyShoot:
                    currentState = GameState.PlayerMove;
                    ResetHumanMove();
                    break;
            }
            TurnDisplayBox.Text = currentState.ToString();
            this.Invalidate(); // Refresh the form to update UI
            clearSelected(true);
        }

        /////////////// Win and lose conditions + Scoring///////////////
        public void ResetGame()
        {
            // Clear all characters from the board
            foreach (var character in playerChars)
            {
                this.Controls.Remove(character.image);
            }
            foreach (var character in enemyChars)
            {
                this.Controls.Remove(character.image);
            }
            playerChars.Clear();
            enemyChars.Clear();
            spaceMarineCount = 0;
            necronCount = 0;
            score++;
            level++;

            // Reset game state
            currentState = GameState.Setup;
            piecesToPlace = 5;
            SetupendBtn.Visible = true;
            SetupendBtn.Enabled = false;
            TurnendBtn.Visible = false;
            nextPhaseBtn.Visible = false;
            TurnDisplayBox.Text = currentState.ToString();

            // Place initial mountains and Necrons on the board
            PlaceMountains(12);
            PlaceNecrons(4+ level);
        }

        private void WriteScoreToFile(int score)
        {
            string filePath = "scores.txt";
            StreamWriter writer = new StreamWriter(filePath, true);
            writer.WriteLine(score);
            writer.Close();
        }
        private void ShowLoseScreen()
        {
            WriteScoreToFile(score);
            MessageBox.Show("You Lose!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Hide();
            Form2 form2 = new Form2(score);
            form2.Show();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            WriteScoreToFile(score);
        }


    }
}