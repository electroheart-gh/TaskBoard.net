using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBoard.net
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(Board.Width, Board.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics gr = Graphics.FromImage(canvas);

            //(10,20)の位置に100x80サイズの黒で塗りつぶされた長方形を描画する
            gr.FillRectangle(Brushes.Black, Board.ClientRectangle);

            //Graphicsオブジェクトのリソースを解放する
            gr.Dispose();
            //PictureBox1に表示する

            Board.Image = canvas;
        }

        private void Board_Click(object sender, EventArgs e)
        {

        }
    }
}
