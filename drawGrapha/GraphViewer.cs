using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace drawGrapha
{

    // 完全独立的 WinForms 弹窗画图器
    public static class GraphViewer
    {
        /// <summary>
        /// 唯一入口：给你 List<List<Element>>，立刻弹窗画图
        /// </summary>
        public static void ShowGraph(List<List<Element>> elementarray)
        {
            if (elementarray == null || elementarray.Count == 0) return;

            // 计算每个节点坐标（简单网格布局）
            var nodePos = LayoutGrid(elementarray);

            // 创建窗体
            var form = new Form
            {
                Text = "Graph Viewer",
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(800, 600),
                AutoScroll = true
            };

            // 创建画布
            var canvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            form.Controls.Add(canvas);

            // 画图事件
            canvas.Paint += (s, e) => DrawGraph(e.Graphics, elementarray, nodePos);

            // 鼠标滚轮放大缩小
            float scale = 1f;
            canvas.MouseWheel += (s, e) =>
            {
                scale += e.Delta > 0 ? 0.1f : -0.1f;
                scale = Math.Max(0.3f, Math.Min(3f, scale));
                canvas.Invalidate();
            };

            // 重绘时带缩放
            canvas.Paint += (s, e) =>
            {
                e.Graphics.ScaleTransform(scale, scale);
                DrawGraph(e.Graphics, elementarray, nodePos);
            };

            // 弹窗
            form.Show();
        }

        // 简易网格布局：行号→y，列号→x
        private static Dictionary<string, PointF> LayoutGrid(List<List<Element>> arr)
        {
            var dic = new Dictionary<string, PointF>();
            float y = 30;
            for (int r = 0; r < arr.Count; r++)
            {
                float x = 30;
                foreach (var e in arr[r])
                {
                    dic[e.ID] = new PointF(x, y);
                    x += 120;          // 水平间隔
                }
                y += 100;              // 垂直间隔
            }
            return dic;
        }

        // 真正的绘制
        private static void DrawGraph(Graphics g, List<List<Element>> arr,
                                      Dictionary<string, PointF> pos)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 画连线
            foreach (var row in arr)
            {
                foreach (var e in row)
                {
                    if (!string.IsNullOrEmpty(e.ORefID) && pos.ContainsKey(e.ORefID))
                    {
                        var p1 = Center(pos[e.ID]);
                        var p2 = Center(pos[e.ORefID]);
                        g.DrawLine(Pens.Black, p1, p2);
                        // 箭头
                        DrawArrow(g, p1, p2);
                    }
                }
            }

            // 画节点
            foreach (var row in arr)
            {
                foreach (var e in row)
                {
                    var loc = pos[e.ID];
                    var rect = new RectangleF(loc.X, loc.Y, 90, 40);
                    g.FillRectangle(Brushes.LightBlue, rect);
                    g.DrawRectangle(Pens.Black, Rectangle.Round(rect));
                    g.DrawString($"{e.TypeText}\n{e.ID}", SystemFonts.DefaultFont,
                                 Brushes.Black, rect, new StringFormat
                                 { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }
        }

        // 矩形中心点
        private static PointF Center(PointF topLeft) =>
            new PointF(topLeft.X + 45, topLeft.Y + 20);

        // 简单箭头
        private static void DrawArrow(Graphics g, PointF from, PointF to)
        {
            float ang = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
            float len = 10;
            var arr = new PointF[]
            {
            new PointF((float)(to.X - len * Math.Cos(ang - 0.3)),
                       (float)(to.Y - len * Math.Sin(ang - 0.3))),
            to,
            new PointF((float)(to.X - len * Math.Cos(ang + 0.3)),
                       (float)(to.Y - len * Math.Sin(ang + 0.3)))
            };
            g.DrawLines(Pens.Black, arr);
        }
    }
}
