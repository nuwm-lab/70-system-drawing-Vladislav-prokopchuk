using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraphDrawingApp
{
    /// <summary>
    /// Головна форма для відображення графіка функції.
    /// </summary>
    public class GraphForm : Form
    {
        private readonly FunctionCalculator calculator;
        private GraphPainter painter;
        private PointF[] graphPoints;

        public GraphForm()
        {
            Text = "Побудова графіка функції";
            Size = new Size(800, 600);
            DoubleBuffered = true; // Запобігає мерехтінню

            // Ініціалізація об’єктів
            calculator = new FunctionCalculator(2.3, 8.3, 0.6);
            graphPoints = calculator.GetPoints();
            painter = new GraphPainter(ClientSize.Width, ClientSize.Height, graphPoints);

            // Підписка на події
            Paint += OnPaint;
            Resize += OnResize;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            painter.DrawGraph(e.Graphics, graphPoints);
        }

        private void OnResize(object sender, EventArgs e)
        {
            painter.UpdateSize(ClientSize.Width, ClientSize.Height);
            Invalidate(); // Викликає перемальовування
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GraphForm());
        }
    }

    /// <summary>
    /// Клас для розрахунку значень функції y = (x+2)^2 / sqrt(x^2+1).
    /// </summary>
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
        /// Обчислює значення функції в точці x.
        /// </summary>
        private double CalculateY(double x)
        {
            return (x + 2) * (x + 2) / Math.Sqrt(x * x + 1);
        }

        /// <summary>
        /// Генерує набір точок для побудови графіка.
        /// </summary>
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

    /// <summary>
    /// Клас для малювання графіка на формі.
    /// </summary>
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

            // Визначення діапазонів для масштабування
            minX = points.Min(p => p.X);
            maxX = points.Max(p => p.X);
            minY = points.Min(p => p.Y);
            maxY = points.Max(p => p.Y);
        }

        /// <summary>
        /// Оновлює розміри області малювання.
        /// </summary>
        public void UpdateSize(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;
        }

        /// <summary>
        /// Малює графік у межах форми з осями та підписами.
        /// </summary>
        public void DrawGraph(Graphics graphics, PointF[] points)
        {
            graphics.Clear(Color.White);

            // Трансформація точок під масштаб форми
            var transformedPoints = points.Select(p => new PointF(
                (p.X - minX) * width / (maxX - minX),
                height - (p.Y - minY) * height / (maxY - minY)
            )).ToArray();

            // Малюємо координатні осі завжди у центрі
            DrawAxes(graphics);

            // Малюємо графік
            using (var graphPen = new Pen(Color.Blue, 2))
            {
                if (transformedPoints.Length > 1)
                {
                    graphics.DrawLines(graphPen, transformedPoints);
                }
            }

            // Підпис формули знизу
            string functionText = "y = (x+2)^2 / sqrt(x^2+1),  2.3 ≤ x ≤ 8.3,  Δx = 0.6";
            var font = new Font("Arial", 12, FontStyle.Italic);
            var textSize = graphics.MeasureString(functionText, font);
            graphics.DrawString(functionText, font, Brushes.Black,
                (width - textSize.Width) / 2, height - textSize.Height - 5);
        }

        /// <summary>
        /// Малює осі X та Y завжди через центр вікна з поділками.
        /// </summary>
        private void DrawAxes(Graphics graphics)
        {
            using (var axisPen = new Pen(Color.Black, 1))
            using (var font = new Font("Arial", 8))
            {
                float xCenter = width / 2f;
                float yCenter = height / 2f;

                // Вісь X
                graphics.DrawLine(axisPen, 0, yCenter, width, yCenter);

                // Вісь Y
                graphics.DrawLine(axisPen, xCenter, 0, xCenter, height);

                // Поділки по X
                for (int i = 0; i <= width; i += 50)
                {
                    graphics.DrawLine(axisPen, i, yCenter - 3, i, yCenter + 3);
                    graphics.DrawString(((i - xCenter) * (maxX - minX) / width + minX).ToString("0.0"),
                        font, Brushes.Black, i - 10, yCenter + 5);
                }

                // Поділки по Y
                for (int j = 0; j <= height; j += 50)
                {
                    graphics.DrawLine(axisPen, xCenter - 3, j, xCenter + 3, j);
                    graphics.DrawString(((height - j - yCenter) * (maxY - minY) / height).ToString("0.0"),
                        font, Brushes.Black, xCenter + 5, j - 7);
                }
            }
        }
    }
}
