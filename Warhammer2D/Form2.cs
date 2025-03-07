using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Formats.Asn1.AsnWriter;

namespace Warhammer2D
{
    public partial class Form2 : Form
    {
        Form1 form1;

        public Form2(int score)
        {
            InitializeComponent();

            int highestScore = GetHighestScore();
            HighScore.Text = ("High Score: " + highestScore.ToString());

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form1 = new Form1();
            form1.FormClosed += Form1_FormClosed;
            form1.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
            int highestScore = GetHighestScore();
            HighScore.Text = ("High Score: " + highestScore.ToString());
        }

        private int GetHighestScore()
        {
            List<int> scores = ReadScoresFromFile();
            BubbleSort(scores);
            return scores.Count > 0 ? scores[0] : 0;
        }

        private List<int> ReadScoresFromFile()
        {
            string filePath = "scores.txt";
            List<int> scores = new List<int>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (int.TryParse(line, out int score))
                    {
                        scores.Add(score);
                    }
                }
            }

            return scores;
        }

        private void BubbleSort(List<int> scores)
        {
            int n = scores.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (scores[j] < scores[j + 1])
                    {
                        int temp = scores[j];
                        scores[j] = scores[j + 1];
                        scores[j + 1] = temp;
                    }
                }
            }
        }
    }
}
