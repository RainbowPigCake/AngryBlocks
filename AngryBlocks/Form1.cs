/* February 27, 2019
 * Angry birds knockoff game
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AngryBlocks
{
    public partial class Form1 : Form
    {
        //Stores the base location for the projectile as an integer
        int originX = 250;
        int originY = 400;

        //Stores the magnitude of gravity and the force it applies
        int gravity = 2;
        int force;

        //Stores whether the user is dragging the projectile and if the projectile is launched
        bool dragging;
        bool launched;

        //Stores the x and y speed that the projectile will travel at on each axis
        int xVelocity;
        int yVelocity;

        //Creates rectangle type variables name
        Rectangle cursorBox, projectileBox, enemyBox, slingshotBox, lifeBox, messageBox;

        //Stores a colour in pen data type to use for onpaint
        Pen blackColour = new Pen(Color.Black);

        //Stores points for a line to be drawn at in onpaint
        Point point1, point2;

        //Stores the amount of lives the user has
        int lives = 3;

        //Stores the time that the projectile has been in motion
        int launchTime;

        //Stores the state of the game eg. win, lose, currently playing and constant integers for easier representation
        int gamestate = PLAYING;
        const int PLAYING = 1;
        const int WIN = 2;
        const int LOSE = 3;

        //Creates button type variable name
        Button btnInstructions, btnPrompt, btnGoal, btnClose;

        //Stores message state and its values for what to show
        int messageState = NONE;
        const int NONE = 0;
        const int INSTRUCTIONS = 1;
        const int PROMPT = 2;
        const int GOALS = 3;

        public Form1()
        {
            InitializeComponent();

            //Runs setup code when the program starts
            Setup();
        }
        //For initializing the program
        void Setup()
        {
            //Code to run when the program starts

            //Sets background image for the form
            this.BackgroundImage = Properties.Resources.backgroundassign1;

            //Assigns location and dimensions for the rectangle boxes
            slingshotBox = new Rectangle(247, 399, 46, 80);
            cursorBox = new Rectangle(0, 0, 10, 10);
            projectileBox = new Rectangle(originX, originY, 40, 40);
            enemyBox = new Rectangle(950, originY - 28, 60, 60);
            lifeBox = new Rectangle(0, 0, 185, 62);
            messageBox = new Rectangle(263, 144, 753, 431);

            //Assigns default values for a line's points
            point1 = new Point(originX + projectileBox.Width / 2, originY + projectileBox.Height / 2);
            point2 = new Point(originX + projectileBox.Width / 2, originY + projectileBox.Height / 2);
            UInterface();

            //Sets default value for force
            force = gravity;
        }
        //For graphics
        protected override void OnPaint(PaintEventArgs e)
        {
            //Onpaint is used for graphics
            base.OnPaint(e);
            if (gamestate == PLAYING)
            {
                //Will check if the user is still playing the game and has not won or lost yet

                //Draws images to rectangle boxes
                e.Graphics.DrawImage(Properties.Resources.slingshottransparent, slingshotBox);
                e.Graphics.DrawImage(Properties.Resources.transparent, cursorBox);
                e.Graphics.DrawImage(Properties.Resources.projectilepic, projectileBox);
                e.Graphics.DrawImage(Properties.Resources.enemyboximg, enemyBox);

                //Draws a line for aesthetics while the user is dragging the elastic back
                e.Graphics.DrawLine(blackColour, point1, point2);

                //Checks the state that the message image should be showing
                switch (messageState)
                {
                    //Will show nothing if nothing is supposed to be shown
                    case NONE:
                        e.Graphics.DrawImage(Properties.Resources.transparent, messageBox);
                        btnClose.Hide();
                        break;
                    //Will show instructions if instructions are supposed to be shown
                    case INSTRUCTIONS:
                        e.Graphics.DrawImage(Properties.Resources.howtoplay1, messageBox);
                        btnClose.Show();
                        break;
                    //Will show the prompt if the prompt is supposed to be shown
                    case PROMPT:
                        e.Graphics.DrawImage(Properties.Resources.prompt1, messageBox);
                        btnClose.Show();
                        break;
                    //Will show the goals if the goals are supposed to be shown
                    case GOALS:
                        e.Graphics.DrawImage(Properties.Resources.objectives1, messageBox);
                        btnClose.Show();
                        break;
                }
            }
            //Always show lives
            switch (lives)
            {
                //Checks how many lives the user has
                //The various cases will update the visual representation for the lives according the the variable amount of lives stored
                case 3:
                    e.Graphics.DrawImage(Properties.Resources._3lives, lifeBox);
                    break;
                case 2:
                    e.Graphics.DrawImage(Properties.Resources._2lives, lifeBox);
                    break;
                case 1:
                    e.Graphics.DrawImage(Properties.Resources._1life, lifeBox);
                    break;
                case 0:
                    e.Graphics.DrawImage(Properties.Resources._0lives, lifeBox);
                    break;
            }
        }
        //For constantly running functions
        private void tmrGeneral_Tick(object sender, EventArgs e)
        {
            //Runs these functions whenever the timer ticks
            Launch();
            Gravity();
            Collision();
            GamestateCheck();
            Debug();
            Refresh();
        }
        //For creating the dragging effect and getting cursor locations
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //Runs whenever the mouse is moved

            if (dragging)
            {
                //If the user is dragging the projectile, update the line to match the projectile 
                //The projectile image will also follow the cursor
                point2 = new Point(projectileBox.X + projectileBox.Width / 2, projectileBox.Y + projectileBox.Height / 2);

                //Only moves projectile while it is on the left side of the screen+
                if (e.X <= 750)
                {
                    projectileBox.Location = new Point(e.X - projectileBox.Width / 2, e.Y - projectileBox.Height / 2);
                }
                //If the mouse is too far to the right side, only move on the y axis
                else
                {
                    projectileBox.Location = new Point(710, e.Y + projectileBox.Height / 2);
                }
            }
            else
            {
                //The points for the line will be equal to its original value when it does not need to be drawn
                point2 = new Point(originX + projectileBox.Width / 2, originY + projectileBox.Height / 2);
            }
            //Invisible box will match the cursor's location which makes it easier to track cursor intersections
            cursorBox.Location = new Point(e.X, e.Y);
        }
        //For checking if the user holds down the mouse while dragging
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //Runs when the left mouse button is pressed down and it is not in the air
            if (e.Button == MouseButtons.Left && !launched)
            {
                if (cursorBox.IntersectsWith(projectileBox))
                {
                    //If the user presses the left mouse button and their cursor is on the projectile they will be dragging the projectile
                    dragging = true;
                }
            }
        }
        //For checking if the user lets go of the left mouse after dragging
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Runs when the left mouse button is let go of
                if (dragging)
                {
                    //When the user is dragging and lets go, they are not dragging anymore
                    dragging = false;

                    //Assign new values to the x and y velocities based on the projectile's displacement while being dragged, scale it down, and launch it
                    xVelocity = (originX - projectileBox.Location.X) / 3;
                    yVelocity = (projectileBox.Location.Y - originY) / 3;
                    launched = true;
                }
            }
        }
        //For having gravity act on the projectile
        void Gravity()
        {
            if (launched)
            {
                //When the projectile has been launched, the force of gravity will increase overtime and act on the projectile
                force += gravity;
                projectileBox.Y += force;
            }
        }
        //For debugging
        void Debug()
        {
            //Temporary function for displaying variables while debugging
            //Temporary labels are put in here while debugging
        }
        //For moving the projectile 
        void Launch()
        {
            if (launched)
            {
                //If the projectile should be launching, it will move on the x and y axis
                projectileBox.X += xVelocity;
                projectileBox.Y -= yVelocity;
            }
        }
        //For checking if the user clicks on the win or loss screen
        private void Form1_Click(object sender, EventArgs e)
        {
            if (gamestate != PLAYING)
            {
                //If the user is not playing anymore and they click on the form, the program will restart
                Application.Restart();
            }
        }
        //For detecting boundary collision, enemy collision, and slingshot collision
        void Collision()
        {
            if (launched)
            {
                if (launchTime > 10)
                {
                    //The following nested if/else if statements will detect whether the projectile leaves the boundaries and if it does, it will reset it
                    if (projectileBox.X < 0)
                    {
                        Reset();
                    }
                    else if (projectileBox.X > this.ClientSize.Width - projectileBox.Width)
                    {
                        Reset();
                    }
                    else if (projectileBox.Y < 0)
                    {
                        Reset();
                    }
                    else if (projectileBox.Y > this.ClientSize.Height - projectileBox.Height)
                    {
                        Reset();
                    }
                }

                //If the projectile touches the enemy the user wins
                if (projectileBox.IntersectsWith(enemyBox))
                {
                    //The user wins
                    gamestate = WIN;
                }

                //Launchtime is counted to ensure that when the projectile hits the slingshot it does not reset instantly
                launchTime += 1;
                if (projectileBox.IntersectsWith(slingshotBox) && launchTime > 10)
                {
                    //If the projectile hits the slingshot and some time has passed reset
                    Reset();
                }
            }
        }
        //For resetting after the projectile leaves boundaries and taking away lives
        void Reset()
        {
            if (lives >= 1)
            {
                //Take life away from player 
                lives -= 1;
            }

            //Puts the projectile to its original location
            projectileBox.Location = new Point(originX, originY);

            //The projectile will not be launching anymore
            launched = false;

            //The force of gravity resets
            force = gravity;

            //The x and y velocities reset along with the launch time
            xVelocity = 0;
            yVelocity = 0;
            launchTime = 0;
        }
        //For checking the gamestate
        void GamestateCheck()
        {
            if (lives <= 0)
            {
                //If the user has 0 lives they lose (greater or equal to zero has been put just in case the user is able to go under 0 lives)
                gamestate = LOSE;
            }
            switch (gamestate)
            {
                //Checks the state of the game
                case LOSE:
                    //Show lose screen when the user loses
                    Lost();
                    break;
                case WIN:
                    //Show win screen when the user wins
                    Won();
                    break;
            }
        }
        //For when the user loses
        void Lost()
        {
            //Timer turns off and stops running functions
            tmrGeneral.Enabled = false;

            //Hides buttons
            btnInstructions.Hide();
            btnPrompt.Hide();
            btnGoal.Hide();
            btnClose.Hide();

            //Display loss screen
            this.BackgroundImage = Properties.Resources.losescreen1;
        }
        //For when the user wins
        void Won()
        {
            //Timer turns off and stops running functions
            tmrGeneral.Enabled = false;

            //Hides buttons
            btnInstructions.Hide();
            btnPrompt.Hide();
            btnGoal.Hide();
            btnClose.Hide();

            //Display win screen
            this.BackgroundImage = Properties.Resources.winscreen1;
        }
        //For displaying user interface
        void UInterface()
        {
            //Creates buttons
            btnInstructions = new Button();
            btnPrompt = new Button();
            btnGoal = new Button();
            btnClose = new Button();

            //Adds the buttons to the list of controls on the form
            this.Controls.Add(btnInstructions);
            this.Controls.Add(btnPrompt);
            this.Controls.Add(btnGoal);
            this.Controls.Add(btnClose);

            //Sets button sizes
            btnInstructions.Size = new Size(44, 44);
            btnPrompt.Size = new Size(44, 44);
            btnGoal.Size = new Size(44, 44);
            btnClose.Size = new Size(44, 44);

            //Gives the buttons locations
            btnInstructions.Location = new Point(lifeBox.Width - 30 + btnInstructions.Width * 1, lifeBox.Height / 2 - 20);
            btnPrompt.Location = new Point(lifeBox.Width - 20 + btnInstructions.Width * 2, lifeBox.Height / 2 - 20);
            btnGoal.Location = new Point(lifeBox.Width - 10 + btnInstructions.Width * 3, lifeBox.Height / 2 - 20);
            btnClose.Location = new Point(messageBox.X + messageBox.Width - 50, messageBox.Y + 5);

            //Sets images for buttons
            btnInstructions.Image = Properties.Resources.btninstructions1;
            btnPrompt.Image = Properties.Resources.btnprompt1;
            btnGoal.Image = Properties.Resources.btngoals1;
            btnClose.Image = Properties.Resources.btnclose1;

            //Adds the button click event to its own function
            btnInstructions.Click += new EventHandler(this.btnInstructions_Click);
            btnPrompt.Click += new EventHandler(this.btnPrompt_Click);
            btnGoal.Click += new EventHandler(this.btnGoal_Click);
            btnClose.Click += new EventHandler(this.btnClose_Click);
        }
        private void btnInstructions_Click(object sender, EventArgs e)
        {
            //When the button is clicked show instructions if it is not already showing. if it is already showing show nothing instead
            if (messageState == INSTRUCTIONS)
            {
                messageState = NONE;
            }
            else
            {
                messageState = INSTRUCTIONS;
            }
        }
        private void btnPrompt_Click(object sender, EventArgs e)
        {
            //When the button is clicked show the prompt if it is not already showing. if it is already showing it show nothing instead
            if (messageState == PROMPT)
            {
                messageState = NONE;
            }
            else
            {
                messageState = PROMPT;
            }
        }
        private void btnGoal_Click(object sender, EventArgs e)
        {
            //When the button is clicked show the goals if it is not already showing. if it is already shwoing it show nothing instead
            if (messageState == GOALS)
            {
                messageState = NONE;
            }
            else
            {
                messageState = GOALS;
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            //Hides messagebox when the close button is clicked
            messageState = NONE;
        }
    }
}
