namespace TextEditor;

public class CustomComponent : Control
{
    public CustomComponent()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        TabStop = true;
        Size = new Size(300, 200);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        using (var redPen = new Pen(Color.Red, 1))
        {
            g.DrawLine(redPen, 0, Height / 2, Width, Height / 2);
            g.DrawLine(redPen, Width / 2, 0, Width / 2, Height);
        }

        using (var font = new Font("Arial", 10))
        using (Brush textBrush = new SolidBrush(Color.Black))
        {
            g.DrawString("Prvi", font, textBrush, new PointF(10, 10));
            g.DrawString("Drugi", font, textBrush, new PointF(10, 30));
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Enter)
        {
            var parent = FindForm();
            parent?.Close();
        }
    }
}