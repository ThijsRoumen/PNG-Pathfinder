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
        public int resolution_height = 1000;
        public int resolution_width = 1000;
        public int resolution_pixel = 1;
        public float threshold = 0.99f;
        public bool debug = true;
        public Main()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.Main_Load);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            processImage();
            Bitmap image = new Bitmap("output.png", true);
            BaseGrid grid = PNGtoGrid(image);
            List<GridPos>path = findPath(grid,DiagonalMovement.Always);
            image = addPath(path,image);
            displayImage(image);
            exportImage(image);
        }
        public void exportImage(Bitmap image){
            image.Save("output.png");
        }
        public List<GridPos> findPath(BaseGrid grid, DiagonalMovement move){
            GridPos startPos = new GridPos(0, 0);
            GridPos endPos = new GridPos(grid.width - 1, grid.height - 1);
            JumpPointParam jpParam = new JumpPointParam(grid, startPos, endPos, true,move , HeuristicMode.EUCLIDEAN);
            List<GridPos> result = JumpPointFinder.FindPath(jpParam); 
            if (debug) Console.WriteLine("found path");
            return result;
        }
        public Bitmap addPath(List<GridPos> path, Bitmap image)
        {
            for (int i = 0; i < path.Count; i ++){
                GridPos curr = path.ElementAt(i);
                GridPos next;
                if (i < path.Count()-1) { next = path.ElementAt(i + 1); }
                else { next = null; }
                if (next == null){
                    image.SetPixel(curr.x, curr.y, Color.Red);
                    if (debug) Console.WriteLine("last pixel drawn");
                    return image;
                }
                using (var graphics = Graphics.FromImage(image))
                {
                    Pen blackPen = new Pen(Color.Red, 1);
                    graphics.DrawLine(blackPen, curr.x, curr.y, next.x, next.y);
                }
            }
            if (debug) Console.WriteLine("drawn path on PNG");
            return image;
        }
        public void displayImage (Bitmap image){
            var pictureBox1 = new PictureBox
            {
                Image = image,
                Size = image.Size
            };

            this.Controls.Add(pictureBox1);
            pictureBox1.BringToFront();
            if (debug) Console.WriteLine("displayed the image");
        }
        public BaseGrid PNGtoGrid(Bitmap input)
        {
            BaseGrid output = new StaticGrid(input.Width, input.Height);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Color c = input.GetPixel(x, y);
                    if (c.GetBrightness() < threshold)
                    { // black, so not walkable
                        output.SetWalkableAt(x, y, false);
                    }
                    else
                    {
                        output.SetWalkableAt(x, y, true);
                    }
                }
            }
            if (debug) Console.WriteLine("converted PNG to grid");
            return output;
        }
        public void processImage (){
            byte[] photoBytes = File.ReadAllBytes("input.png");
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new PngFormat { Quality = 100 };
            Console.WriteLine("start doing shit");
            Size size = new Size(resolution_width,resolution_height);
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Saturation(-100)
                                    .Pixelate(resolution_pixel)
                                    .Filter(MatrixFilters.BlackWhite)
                                    .Save(outStream);
                    }
                    // Do something with the stream.
                    File.WriteAllBytes("output.png", outStream.ToArray());
                }
            }
            if (debug) Console.WriteLine("finished processing image");
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(resolution_width, resolution_height);
            this.Name = "Output";
            this.Text = "Output";
            this.ResumeLayout(false);
        }
    }
}