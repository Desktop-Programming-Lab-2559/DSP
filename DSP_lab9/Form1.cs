﻿using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using DSPbase;
using GoodRBF;

namespace DSP_lab9
{
    public partial class Form1 : Form
    {
        private int size;
        private int height;
        private int width;
        private RBFGrid recognizer;
        private int elementWidth, elementHeight;

        //private RBF recognizer;

        private int[] draws;
        private Bitmap image;

        public Form1()
        {
            this.InitializeComponent();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.draws[(e.Y / this.elementHeight) * this.width + (e.X / this.elementWidth)] = 1;
            }
            else
            {
                this.draws[(e.Y / this.elementHeight) * this.width + (e.X / this.elementWidth)] = -1;
            }
            this.panel1.Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.panel1.CreateGraphics();
            for (int i = 0; i < this.size; i++)
            {
                if (this.draws[i] == 1)
                {
                    int coordinateX = (i % this.width) * this.elementWidth;
                    int coordinateY = (i / this.width) * this.elementHeight;
                    g.FillRectangle(Brushes.Black, coordinateX, coordinateY, this.elementWidth, this.elementHeight);
                }
            }
        }

        private void DrawTheImage()
        {
            ImageRecognizer imageRecongizer = new ImageRecognizer(this.image, 128, 0);
            imageRecongizer.TransformToBlackAndWhite();
            this.image = imageRecongizer.OutputImage;
            for (int i = 0; i < this.width; i++)
            {
                for (int j = 0; j < this.height; j++)
                {
                    if (this.image.GetPixel(i, j).R == 0)
                    {
                        this.draws[j * this.width + i] = 1;
                    }
                    else
                    {
                        this.draws[j * this.width + i] = -1;
                    }
                }
            }

            this.panel1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp,*.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png"
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stream myStream;
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            this.image = new Bitmap(openFileDialog1.FileName);
                            if (this.image.Height > 100 || this.image.Width > 100)
                            {
                                throw new DataException("Image too big");
                            }
                            if (this.size == 0)
                            {
                                this.size = this.image.Width * this.image.Height;
                                this.height = this.image.Height;
                                this.width = this.image.Width;
                                this.draws = new int[this.size];
                                this.elementHeight = this.panel1.Height / this.height;
                                this.elementWidth = this.panel1.Width / this.width;
                                this.panel1.Width = this.elementWidth * this.width;
                                this.panel1.Height = this.elementHeight * this.height;
                            }
                            else
                            {
                                if (this.height != this.image.Height || this.width != this.image.Width)
                                {
                                    throw new DataException("Image doesn't match the parameters");
                                }
                            }
                            this.DrawTheImage();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.recognizer == null)
            {
                this.recognizer = new RBFGrid();
                this.recognizer.InputCellCount = height * width;
                this.recognizer.OutputCellCount = 3;
            }
            if (this.size == 0)
            {
                MessageBox.Show("No image to teach ");
                return;
            }

            int group = 0;
            if (radioButton2.Checked)
            {
                group = 1;
            }
            if (radioButton3.Checked)
            {
                group = 2;
            }

            recognizer.Input = BMPTransform.BitmapToDouble(GetBitmap(draws));
            recognizer.LearnImage(group);
            MessageBox.Show("Success");
        }

        private Bitmap GetBitmap(int[] input)
        {
            if (input.Length % 4 != 0) return null;
            Bitmap output = new Bitmap(width, height);
            BitmapData outputData = output.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppRgb
                );

            System.Runtime.InteropServices.Marshal.Copy(input, 0, outputData.Scan0, input.Length);
            output.UnlockBits(outputData);
            return output;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.recognizer == null)
            {
                MessageBox.Show("Error: no recognizer implemented");
                return;
            }


            recognizer.Input = BMPTransform.BitmapToDouble(GetBitmap(draws));
            recognizer.DoWork();

            label2.Text = recognizer.Output[0].ToString();
            label4.Text = recognizer.Output[1].ToString();
            label6.Text = recognizer.Output[2].ToString();

            double maxValue = recognizer.Output[0];
            int index = 0;
            for (int i = 1; i < 3; i++)
            {

                if (recognizer.Output[i] > maxValue)
                {
                    maxValue = recognizer.Output[i];
                    index = i;
                }
            }

            MessageBox.Show("Group number is " + (index+1).ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.size; i++)
            {
                this.draws[i] = -1;
            }
            this.panel1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.size; i++)
            {
                this.draws[i] = -1 * this.draws[i];
            }
            this.panel1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            recognizer.EndLearn();
            MessageBox.Show("Success");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
