using System.Numerics;

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

        // player pieces on the board
        private List<Character> playerChars = new List<Character>();
        private List<Character> enemyChars = new List<Character>();
        HashSet<Point> mountainPositions = new HashSet<Point>();
        public Character playerSelected;
        public GameState currentState = GameState.Setup;
        private int piecesToPlace = 4;

        // Random number for generating positions for enemies start
        private Random random = new Random();

        // Keeps track of how many units there are on both sides
        private int spaceMarineCount = 0;
        private int necronCount = 0;

        // Variables for grid and UI layout
        int MX = 10;
        int MY = 10;
        int squaresize = 50;
        int highwidth = 10;
        Rectangle[] boxes;

        // Items on screen
        Button SetupendBtn;
        Button TurnendBtn;
        TextBox TurnDisplayBox;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Click += Form_Click;
            this.Paint += GridLines;
            this.MouseMove += MouseMoved;

            // Initialize setup and turn buttons
            SetupEndButton();
            EndTurnButton();

            // Place initial mountains and Necrons on the board
            PlaceMountains(3);
            PlaceNecrons(5); // maximum 15 or game crashes

            TurnDisplay();
        }

        // Creates and configures the "End Setup" button
        private void SetupEndButton()
        {
            SetupendBtn = new Button();
            SetupendBtn.Text = "End Setup";
            SetupendBtn.Size = new Size(100, 30);
            SetupendBtn.Location = new Point(highwidth * squaresize + 20, 10);
            SetupendBtn.Click += SetupEndBtn_Click;
            SetupendBtn.Enabled = false;
            this.Controls.Add(SetupendBtn);
        }

        // Creates and configures the "End Turn" button
        private void EndTurnButton()
        {
            TurnendBtn = new Button();
            TurnendBtn.Text = "End Turn";
            TurnendBtn.Size = new Size(100, 30);
            TurnendBtn.Location = new Point(highwidth * squaresize + 20, 10);
            TurnendBtn.Click += EndTurnBtn_Click;
            TurnendBtn.Enabled = false;
            TurnendBtn.Visible = false;
            this.Controls.Add(TurnendBtn);
        }

        private void TurnDisplay()
        {
            TurnDisplayBox = new TextBox();
            TurnDisplayBox.Text = currentState.ToString();
            TurnDisplayBox.Size = new Size(100, 50);
            TurnDisplayBox.Location = new Point(highwidth * squaresize + 20, 230);
            TurnDisplayBox.TextAlign = HorizontalAlignment.Center;
            TurnDisplayBox.Enabled = false;
            this.Controls.Add(TurnDisplayBox);
        }

        //  "End Setup" button click
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
                    currentState = GameState.PlayerMove; // Transition to PlayerMove state
                    SetupendBtn.Text = "Game Active";
                    SetupendBtn.Visible = false;
                    return;
                }

            }
            currentState = GameState.PlayerMove; // Transition to PlayerMove state
            SetupendBtn.Text = "Game Active";
            SetupendBtn.Visible = false;
            TurnendBtn.Visible = true;
            TurnendBtn.Enabled = true;
            TurnDisplayBox.Text = currentState.ToString();
        }

        // "End Turn" button click
        private void EndTurnBtn_Click(object sender, EventArgs e)
        {
            // Switch to enemy move phase
            currentState = GameState.EnemyMove;
            MoveNecrons();

            // goes back to PlayerMove phase
            TurnendBtn.Enabled = true;
            currentState = GameState.PlayerMove;
            TurnDisplayBox.Text = currentState.ToString();
            ResetHumanMove();
        }

        private void MoveNecrons()
        {
            HashSet<Point> usedPositions = mountainPositions;
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
        }

        public void clearSelected()
        {
            foreach (Character c in playerChars)
            {
                c.isSelected = false;
                c.image.BorderStyle = BorderStyle.None;
            }
        }

        // click events on the form during setup or player movement
        private void Form_Click(object sender, EventArgs e)
        {
            if (currentState == GameState.Setup)
            {
                // Allow placing players only in the last two rows
                if (MY >= ((highwidth - 3) * squaresize) && MX < 500)
                {
                    piecesToPlace--;
                    // Enable the "End Setup" button once all pieces are placed
                    if (spaceMarineCount >= 0)
                    {
                        SetupendBtn.Enabled = true;
                    }
                    // Add the player piece to the list and dictionary and adds them to a count
                    spaceMarineCount++; // Increment the count for Space Marines
                    Character plyr = new Character(MX, MY, Properties.Resources.Space_marine_Background_Removed, squaresize, this);
                    playerChars.Add(plyr);
                    if (piecesToPlace <= 0)
                    {
                        currentState = GameState.PlayerMove;
                        SetupendBtn.Text = "Game Active";
                        SetupendBtn.Visible = false;
                        TurnendBtn.Visible = true;
                        TurnendBtn.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("You can only place players in the last two rows.", "Placement Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if ((currentState == GameState.PlayerMove) && (playerSelected != null))
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
                    if (c == playerSelected)
                    {
                        c.hasMoved = true;
                        c.image.Location = new Point(MX,MY);
                        clearSelected();
                    }
                    if (c.hasMoved)
                    {
                        playersMoved++;
                        if (playersMoved == 4)
                        {
                            // Switch to enemy move phase
                            currentState = GameState.EnemyMove;
                            MoveNecrons();
                            // goes back to PlayerMove phase
                            TurnendBtn.Enabled = true;
                            currentState = GameState.PlayerMove;
                            TurnDisplayBox.Text = currentState.ToString();
                            ResetHumanMove();
                        }
                    }
                }
                playerSelected = null;
            }
            TurnDisplayBox.Text = currentState.ToString();
            this.Invalidate();
        }

        // Draws the grid and highlights things od relevance
        private void GridLines(object sender, PaintEventArgs e)
        {
            int totalBoxes = highwidth * highwidth;

            // Draw grid lines
            for (int i = 0; i < highwidth + 1; i++)
            {
                e.Graphics.DrawLine(Pens.Red, i * squaresize, 0, i * squaresize, squaresize * highwidth);
                e.Graphics.DrawLine(Pens.Red, 0, i * squaresize, squaresize * highwidth, i * squaresize);
            }

            // Highlight placement area during setup
            if (currentState == GameState.Setup)
            {
                if (MX < 500)
                {
                    e.Graphics.DrawRectangle(Pens.Green, MX, MY, squaresize, squaresize);
                }
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
                } while (mountainPositions.Contains(mountainLocation)); // Makes sure there are no duplicates'

                mountainPositions.Add(mountainLocation);
                PictureBox mountain = new PictureBox
                {
                    Size = new Size(squaresize - 2, squaresize - 2),
                    Location = mountainLocation,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Properties.Resources.mountain_pixel_removebg_preview
                };
                this.Controls.Add(mountain);
            }
        }

        // Places a specified number of Necrons on the board
        private void PlaceNecrons(int count)
        {
            HashSet<Point> usedPositions = mountainPositions;
            for (int i = 0; i < count; i++)
            {
                Point necronLocation;
                do
                {
                    // Generate random position for the Necrons to move to
                    int randomX = random.Next(0, highwidth) * squaresize + 1;
                    int randomY = random.Next(0, 2) * squaresize + 1;
                    necronLocation = new Point(randomX, randomY);
                } while (usedPositions.Contains(necronLocation)); // Makes sure there are no duplicates'

                usedPositions.Add(necronLocation);
                Character nec = new Character(necronLocation.X, necronLocation.Y, Properties.Resources.necron, squaresize, this);
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
    }
}