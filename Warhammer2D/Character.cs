using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using static Warhammer2D.Form1;

namespace Warhammer2D
{
    public class Character
    {
        public PictureBox image;
        public bool hasMoved = false;
        public bool hasShot = false;
        public int health = 100;
        public bool isSelected = false;
        private Form1 parentForm;
        public bool isPlayer;

        public Character(int x, int y, Bitmap img, int width, bool isplayer, Form1 frm)
        {
            image = new PictureBox();
            image.Size = new System.Drawing.Size(width - 2, width - 2);
            image.SizeMode = PictureBoxSizeMode.StretchImage;
            image.Location = new System.Drawing.Point(x, y);
            image.Image = img;
            image.Click += clickEvent;
            image.BackColor = Color.Transparent;
            parentForm = frm;
            frm.Controls.Add(image);
            isPlayer = isplayer;
        }

        private void clickEvent(object sender, EventArgs e)
        {
            if ((parentForm.currentState == GameState.PlayerMove) && !isPlayer)
            {
                return;
            }
            
            if ((parentForm.currentState == GameState.PlayerMove) || (parentForm.currentState == GameState.PlayerShoot))
            {
                if (isSelected == true)
                {
                    isSelected = false;
                    parentForm.clearSelected(isPlayer);
                }
                else
                {
                    isSelected = true;
                    //parentForm.clearSelected();
                    parentForm.playerSelected = this;
                    image.BorderStyle = BorderStyle.Fixed3D;
                }
                if (parentForm.currentState == GameState.PlayerShoot)
                {
                    if ((isPlayer == false))
                        parentForm.target = this;
                    else
                        parentForm.shooter = this;
                }
            }
        }

        public void CPUmove(HashSet<Point> usedPositions, int stepSize)
        {
            hasMoved = true;

            int newx = image.Location.X;
            int newy = image.Location.Y + stepSize;

            if (newy < 750)
            {
                //need to make better just moves down by 1 sqaure
                while (usedPositions.Contains(new Point(newx, newy)))
                {
                    newy = newy + stepSize;
                }
                image.Location = new Point(newx, newy);
            }
            else
            {
                return;
            }
            
        }

        public void Shoot(Character target)
        {
            if (target != null)
            {
                target.health -= 200; // Example damage value
                if (target.health <= 0)
                {
                    target.health = 0;
                    // Handle character death (e.g., remove from the board)
                    target.image.Visible = false;
                    if (target.isPlayer)
                    {
                        parentForm.playerChars.Remove(target);
                        parentForm.spaceMarineCount--;
                        
                    }
                    else
                    {
                        parentForm.enemyChars.Remove(target);
                        parentForm.necronCount--;
                        if (parentForm.necronCount == 0)
                        {
                            parentForm.ResetGame();
                        }
                    }
                }
            }
            parentForm.clearSelected(isPlayer);
            parentForm.clearSelected(isSelected);
        }
    }
}
