using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor;
using EpPathFinding.cs;

namespace imagefolding
{
    public partial class Main : Form
    {
        public static int resolution_height = 1000;
        public static int resolution_width = 1000;
        public static int resolution_pixel = 1;
        public GridPos pathStart = new GridPos(0, 0);
        public GridPos pathEnd = new GridPos(resolution_height - 1, resolution_width - 1);
        public float threshold = 0.99f;
        public bool debug = false;
        public Main()
        {
            InitializeComponent();
            this.Load += new EventHandler(this.Main_Load);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            processImage();
            Bitmap image = new Bitmap(
                "output.png", 
                true
            );
            BaseGrid grid = PNGtoGrid(image);
            List<GridPos>path = findPath(
                grid,
                DiagonalMovement.Always, 
                pathStart, 
                pathEnd
            );
            image = addPath(path,image);
            displayImage(image);
            exportImage(image);
        }
        public void exportImage(Bitmap image){
            image.Save("output.png");
        }
        public List<GridPos> findPath(BaseGrid grid, DiagonalMovement move, GridPos startPos, GridPos endPos){
            JumpPointParam jpParam = new JumpPointParam(
                grid, 
                startPos, 
                endPos, 
                true,
                move, 
                HeuristicMode.EUCLIDEAN
            );
            List<GridPos> result = JumpPointFinder.FindPath(jpParam); 
            if (debug) Console.WriteLine("found path");
            return result;
        }
        public Bitmap addPath(List<GridPos> path, Bitmap image)
        {
            for (int i = 0; i < path.Count; i ++){
                GridPos curr = path.ElementAt(i);
                GridPos next;
                if (i < path.Count()-1) 
                    next = path.ElementAt(i + 1);
                else 
                    next = null;
                if (next == null){
                    image.SetPixel(curr.x,
                                   curr.y, 
                                   Color.Red
                                  );
                    if (debug) Console.WriteLine("last pixel drawn");
                    return image;
                }
                using (var graphics = Graphics.FromImage(image))
                {
                    Pen blackPen = new Pen(Color.Red, 1);
                    graphics.DrawLine(blackPen, 
                                      curr.x, 
                                      curr.y, 
                                      next.x, 
                                      next.y
                                     );
                }
            }
            if (debug) Console.WriteLine("drawn path on PNG");
            return image;
        }
        public void displayImage (Bitmap image){
            var display = new PictureBox
            {
                Image = image,
                Size = image.Size
            };
            this.Controls.Add(display);
            display.BringToFront();
            if (debug) Console.WriteLine("displayed the image");
        }
        public BaseGrid PNGtoGrid(Bitmap input)
        {
            BaseGrid output = new StaticGrid(
                input.Width, 
                input.Height
            );
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Color c = input.GetPixel(
                        x, 
                        y
                    );
                    if (c.GetBrightness() < threshold)
                        output.SetWalkableAt(
                            x, 
                            y, 
                            false
                        );
                    else
                        output.SetWalkableAt(
                            x, 
                            y, 
                            true
                        );
                }
            }
            if (debug) Console.WriteLine("converted PNG to grid");
            return output;
        }
        public void processImage (){
            byte[] photoBytes = File.ReadAllBytes("input.png");
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new PngFormat { Quality = 100 };
            Size size = new Size(
                resolution_width,
                resolution_height
            );
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Saturation(-100)
                                    .Pixelate(resolution_pixel)
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Save(outStream);
                    }
                    File.WriteAllBytes("output.png", outStream.ToArray());
                }
            }
            if (debug) Console.WriteLine("finished processing image");
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(
                6F, 
                13F
            );
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(
                resolution_width, 
                resolution_height
            );
            this.Name = "Output";
            this.Text = "Output";
            this.ResumeLayout(false);
        }
    }
}