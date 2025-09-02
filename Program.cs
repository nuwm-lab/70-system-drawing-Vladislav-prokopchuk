using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraphDrawingApp
{
    public class GraphForm : Form
    {
        private readonly FunctionCalculator calculator;
        private GraphPainter painter;
        private PointF[] graphPoints;

        private TextBox inputBox;
        private Button calculateButton;
        private Label resultLabel;

        public GraphForm()
        {
            Text = "Побудова графіка функції";
            Size = new Size(800, 600);
            DoubleBuffered = true;

            // Калькулятор і графік
            calculator = new FunctionCalculator(2.3, 8.3, 0.6);
            graphPoints = calculator.GetPoints();
            painter = new GraphPainter(ClientSize.Width, ClientSize.Height, graphPoints);

            // Елементи управління
            inputBox = new TextBox { Location = new Point(10, 10), Width = 100 };
            calculateButton = new Button { Text = "Обчислити", Location = new Point(120, 8) };
            resultLabel = new Label { Text = "Результат: ", Location = new Point(220, 12), AutoSize = true };

            calculateButton.Click += OnCalculateClick;

            Controls.Add(inputBox);
            Controls.Add(calculateButton);
            Controls.Add(resultLabel);

            Paint += OnPaint;
            Resize += OnResize;
        }

        private void OnCalculateClick(object sender, EventArgs e)
        {
            if (double.TryParse(inputBox.Text, out double inputValue))
            {
                double result = calculator.CalculateY(inputValue);
                resultLabel.Text = $"Результат: y = {result:F4}";
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть число.", "Помилка вводу",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            painter.DrawGraph(e.Graphics, graphPoints);
        }

        private void OnResize(object sender, EventArgs e)
        {
            painter.UpdateSize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GraphForm());
        }
    }

    public class FunctionCalculator
    {
        private readonly double startX;
        private readonly double endX;
        private readonly double deltaX;

        public FunctionCalculator(double startX, double endX, double deltaX)
        {
            this.startX = startX;
            this.endX = endX;
            this.deltaX = deltaX;
        }

        /// <summary>
        /// Обчислює значення функції y = (x+2)^2 / sqrt(x^2+1).
        /// </summary>
        public double CalculateY(double inputValue)
        {
            return Math.Pow(inputValue + 2, 2) / Math.Sqrt(Math.Pow(inputValue, 2) + 1);
        }

        public PointF[] GetPoints()
        {
            var points = new List<PointF>();
            for (double x = startX; x <= endX; x += deltaX)
            {
                points.Add(new PointF((float)x, (float)CalculateY(x)));
            }
            return points.ToArray();
        }
    }

    public class GraphPainter
    {
        private int width;
        private int height;
        private readonly float minX;
        private readonly float maxX;
        private readonly float minY;
        private readonly float maxY;

        public GraphPainter(int width, int height, PointF[] points)
        {
            UpdateSize(width, height);

            minX = points.Min(p => p.X);
            maxX = points.Max(p => p.X);
            minY = points.Min(p => p.Y);
            maxY = points.Max(p => p.Y);
        }

        public void UpdateSize(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;
        }

        public void DrawGraph(Graphics graphics, PointF[] points)
        {
            graphics.Clear(Color.White);

            var transformedPoints = points.Select(p => new PointF(
                (p.X - minX) * width / (maxX - minX),
                height - (p.Y - minY) * height / (maxY - minY)
            )).ToArray();

            DrawAxes(graphics);

            using (var graphPen = new Pen(Color.Blue, 2))
            {
                if (transformedPoints.Length > 1)
                {
                    graphics.DrawLines(graphPen, transformedPoints);
                }
            }

            string functionText = "y = (x+2)^2 / sqrt(x^2+1),  2.3 ≤ x ≤ 8.3,  Δx = 0.6";
            var font = new Font("Arial", 12, FontStyle.Italic);
            var textSize = graphics.MeasureString(functionText, font);
            graphics.DrawString(functionText, font, Brushes.Black,
                (width - textSize.Width) / 2, height - textSize.Height - 5);
        }

        private void DrawAxes(Graphics graphics)
        {
            using (var axisPen = new Pen(Color.Black, 1))
            using (var font = new Font("Arial", 8))
            {
                float xCenter = width / 2f;
                float yCenter = height / 2f;

                graphics.DrawLine(axisPen, 0, yCenter, width, yCenter);
                graphics.DrawLine(axisPen, xCenter, 0, xCenter, height);

                for (int i = 0; i <= width; i += 50)
                {
                    graphics.DrawLine(axisPen, i, yCenter - 3, i, yCenter + 3);
                }

                for (int j = 0; j <= height; j += 50)
                {
                    graphics.DrawLine(axisPen, xCenter - 3, j, xCenter + 3, j);
                }
            }
        }
    }
}
