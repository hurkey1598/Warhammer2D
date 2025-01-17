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

        public Character(int x, int y, Bitmap img, int width, Form1 frm)
        {
            Graphics g;
            image = new PictureBox();
            image.Size = new System.Drawing.Size(width - 2, width - 2);
            image.SizeMode = PictureBoxSizeMode.StretchImage;
            image.Location = new System.Drawing.Point(x, y);
            image.Image = img;
            image.Click += clickEvent;
            parentForm = frm;
            frm.Controls.Add(image);
        }

        private void clickEvent(object sender, EventArgs e)
        {
            if (parentForm.currentState != GameState.PlayerMove)
                return;

            if (isSelected == true)
            {
                isSelected = false;
                parentForm.clearSelected();
            }
            else
            {
                isSelected = true;
                parentForm.clearSelected();
                parentForm.playerSelected = this;
                image.BorderStyle = BorderStyle.Fixed3D;
            }
        }

        public void CPUmove(HashSet<Point> usedPositions, int stepSize)
        {
            hasMoved = true;

            int newx = image.Location.X;
            int newy = image.Location.Y+ stepSize;
            //need to make better just moves down by 1 sqaure
            while (usedPositions.Contains(new Point(newx,newy)))
            {
                newx = newx;
                newy = newy + stepSize;
            }
            image.Location = new Point(newx,newy);
        }
    }
}
