using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Lab_rabota_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private class Point
        {
            public float x, y, z, H;
        }

        private class Vector
        {
            public float x, y, z;
        }

        private class Plane
        {
            public List<int> vertices = new List<int>();
            public bool visibility = false;

            public void SetVisibility(bool value)
            {
                visibility = value;
            }
        }
        List<Point> fp = new List<Point>();
        List<Plane> fl = new List<Plane>();

        private void Build()
        {
            Graphics canvas = pictureBox1.CreateGraphics();
            canvas.Clear(Color.White);
            canvas.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            canvas.ScaleTransform(1, -1);
            Pen thePen = new Pen(Color.Black);

            ResetVisibility();
            CheckVisibility();

            foreach (Plane thePlane in fl)
            {
                if (!thePlane.visibility)
                {
                    continue;
                }

                for (int i = 0; i < thePlane.vertices.Count; i++)
                {
                    Point current = fp[thePlane.vertices[i] - 1];
                    Point previous = new Point();
                    if (i == 0)
                    {
                        previous = fp[thePlane.vertices.Last() - 1];
                    }
                    else
                    {
                        previous = fp[thePlane.vertices[i - 1] - 1];
                    }

                    canvas.DrawLine(thePen, (current.x / current.H), (current.y / current.H),
                                            (previous.x / previous.H), (previous.y / previous.H));
                }
            }
        }

        private void ResetVisibility()
        {
            foreach (Plane thePlane in fl)
            {
                thePlane.SetVisibility(false);
            }
        }

        private void CheckVisibility()
        {
            Vector gazeDirection = new Vector
            {
                x = 0,
                y = 0,
                z = -1
            };

            foreach (Plane thePlane in fl)
            {
                Point firstPoint = fp[thePlane.vertices[0] - 1];
                Point secondPoint = fp[thePlane.vertices[1] - 1];
                Point thirdPoint = fp[thePlane.vertices[2] - 1];

                Vector firstVector = new Vector
                {
                    x = secondPoint.x / secondPoint.H - firstPoint.x / firstPoint.H,
                    y = secondPoint.y / secondPoint.H - firstPoint.y / firstPoint.H,
                    z = secondPoint.z / secondPoint.H - firstPoint.z / firstPoint.H
                };
                Vector secondVector = new Vector
                {
                    x = thirdPoint.x / thirdPoint.H - secondPoint.x / secondPoint.H,
                    y = thirdPoint.y / thirdPoint.H - secondPoint.y / secondPoint.H,
                    z = thirdPoint.z / thirdPoint.H - secondPoint.z / secondPoint.H
                };
                Vector normal = VectorProduct(firstVector, secondVector);

                Point externalPoint = new Point();
                for (int j = 1; j <= fp.Count; j++)
                {
                    if (!thePlane.vertices.Contains(j))
                    {
                        externalPoint = fp[j - 1];
                        break;
                    }
                }
                Vector externalVector = new Vector
                {
                    x = externalPoint.x / externalPoint.H - firstPoint.x / firstPoint.H,
                    y = externalPoint.y / externalPoint.H - firstPoint.y / firstPoint.H,
                    z = externalPoint.z / externalPoint.H - firstPoint.z / firstPoint.H
                };

                if (ScalarProduct(normal, externalVector) > 0)
                {
                    normal.x = -normal.x;
                    normal.y = -normal.y;
                    normal.z = -normal.z;
                }

                if (ScalarProduct(normal, gazeDirection) < 0)
                {
                    thePlane.SetVisibility(true);
                }
            }
        }

        private Vector VectorProduct(Vector first, Vector second)
        {
            Vector product = new Vector
            {
                x = first.y * second.z - first.z * second.y,
                y = first.z * second.x - first.x * second.z,
                z = first.x * second.y - first.y * second.x
            };
            return product;
        }

        private float ScalarProduct(Vector first, Vector second)
        {
            float product = first.x * second.x + first.y * second.y + first.z * second.z;
            return product;
        }

        private List<Point> PointsTransform(List<Point> list, float[,] matrix)
        {
            List<Point> result = new List<Point>();

            foreach (Point dot in list)
            {
                Point dotTransformed = new Point
                {
                    x = dot.x * matrix[0, 0] + dot.y * matrix[1, 0] + dot.z * matrix[2, 0] + dot.H * matrix[3, 0],
                    y = dot.x * matrix[0, 1] + dot.y * matrix[1, 1] + dot.z * matrix[2, 1] + dot.H * matrix[3, 1],
                    z = dot.x * matrix[0, 2] + dot.y * matrix[1, 2] + dot.z * matrix[2, 2] + dot.H * matrix[3, 2],
                    H = dot.x * matrix[0, 3] + dot.y * matrix[1, 3] + dot.z * matrix[2, 3] + dot.H * matrix[3, 3]
                };
                result.Add(dotTransformed);
            }

            return result;
        }

        private void Загрузка_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            string figure = dialog.FileName;
            StreamReader reader = new StreamReader(figure);

            fp.Clear();
            fl.Clear();

            int NumberPo = Convert.ToInt32(reader.ReadLine());
            for (int i = 0; i < NumberPo; i++)
            {
                string line = reader.ReadLine();
                string[] coords = line.Split(' ');

                Point dot = new Point
                {
                    x = float.Parse(coords[0]),
                    y = float.Parse(coords[1]),
                    z = float.Parse(coords[2]),
                    H = float.Parse(coords[3])
                };
                fp.Add(dot);
            }

            int NumberPl = Convert.ToInt32(reader.ReadLine());
            for (int i = 0; i < NumberPl; i++)
            {
                string line = reader.ReadLine();
                string[] verticesArray = line.Split(' ');

                List<int> verticesList = new List<int>();
                foreach (string vertex in verticesArray)
                {
                    verticesList.Add(Convert.ToInt32(vertex));
                }

                Plane plane = new Plane
                {
                    vertices = verticesList
                };
                fl.Add(plane);
            }

            reader.Close();
        }

        private void Построение_Click(object sender, EventArgs e)
        {
            Build();
        }

        private void Перенос_Click(object sender, EventArgs e)
        {
            float aX = float.Parse(textbxx.Text);
            float aY = float.Parse(textbxy.Text);
            float aZ = float.Parse(textbxz.Text);

            float[,] matrix = new float[,]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {aX, aY, aZ, 1}
            };

            fp = PointsTransform(fp, matrix);
            Build();
        }

        private void Масштаб_Click(object sender, EventArgs e)
        {
            float kX = float.Parse(tbx.Text);
            float kY = float.Parse(tby.Text);
            float kZ = float.Parse(tbz.Text);

            float[,] matrix = new float[,]
            {
                {kX, 0, 0, 0},
                {0, kY, 0, 0},
                {0, 0, kZ, 0},
                {0, 0, 0, 1}
            };

            fp = PointsTransform(fp, matrix);
            Build();
        }

        private void Поворот_Click(object sender, EventArgs e)
        {
            float alphaX = float.Parse(tba.Text) * (float)Math.PI / 180;
            float bettaY = float.Parse(tbb.Text) * (float)Math.PI / 180;
            float gammaZ = float.Parse(tbg.Text) * (float)Math.PI / 180;

            if (alphaX != 0)
            {
                float[,] matrixX = new float[,]
                {
                    {1, 0, 0, 0},
                    {0, (float)Math.Cos(alphaX), (float)Math.Sin(alphaX), 0},
                    {0, -(float)Math.Sin(alphaX), (float)Math.Cos(alphaX), 0},
                    {0, 0, 0, 1}
                };

                fp = PointsTransform(fp, matrixX);
            }

            if (bettaY != 0)
            {
                float[,] matrixY = new float[,]
                {
                    {(float)Math.Cos(bettaY), 0, -(float)Math.Sin(bettaY), 0},
                    {0, 1, 0, 0},
                    {(float)Math.Sin(bettaY), 0, (float)Math.Cos(bettaY), 0},
                    {0, 0, 0, 1}
                };

                fp = PointsTransform(fp, matrixY);
            }

            if (gammaZ != 0)
            {
                float[,] matrixZ = new float[,]
                {
                    {(float)Math.Cos(gammaZ), (float)Math.Sin(gammaZ), 0, 0},
                    {-(float)Math.Sin(gammaZ), (float)Math.Cos(gammaZ), 0, 0},
                    {0, 0, 1, 0},
                    {0, 0, 0, 1}
                };

                fp = PointsTransform(fp, matrixZ);
            }

            Build();
        }

        private void ОПП_Click(object sender, EventArgs e)
        {
            float fX = float.Parse(textbx.Text);
            float fY = float.Parse(textby.Text);
            float fZ = float.Parse(textbz.Text);

            if (fX != 0 &
                fY != 0 &
                fZ != 0)
            {
                float[,] matrix = new float[,]
                {
                    {1, 0, 0, 1 / fX},
                    {0, 1, 0, 1 / fY},
                    {0, 0, 1, 1 / fZ},
                    {0, 0, 0, 1}
                };

                fp = PointsTransform(fp, matrix);
                Build();
            }
            else
            {
                MessageBox.Show("Невозможно использовать 0");
            }
        }

        private void Сдвиг_Click(object sender, EventArgs e)
        {
            float K = float.Parse(textbk.Text);

            float[,] matrix = new float[,]
            {
                {1, K, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            };

            fp = PointsTransform(fp, matrix);
            Build();
        }

        private void Вмещение_Click(object sender, EventArgs e)
        {
            float maxX = fp.Max(P => P.x);
            float maxY = fp.Max(P => P.y);

            float kX = (pictureBox1.Width / 2) / maxX;
            float kY = (pictureBox1.Height / 2) / maxY;
            float k = Math.Min(kX, kY);

            float[,] matrix = new float[,]
            {
                {k, 0, 0, 0},
                {0, k, 0, 0},
                {0, 0, k, 0},
                {0, 0, 0, 1}
            };

            fp = PointsTransform(fp, matrix);
            Build();
        }
    }
}