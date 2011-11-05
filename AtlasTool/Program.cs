using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;

namespace AtlasTool
{
    class AtlasCommandLineTool
    {
        string[] args;
        string outputFile;
        string outputDir;
        string inputDir;
        List<int> checkedArgs = new List<int>();
        Dictionary<string, Image> _images = new Dictionary<string, Image>();
        List<Image> _sortedImages = new List<Image>();
        List<string> _sortedImageNames = new List<string>();
        List<string> _imageNames = new List<string>();
        Size _maxAtlasSize = new Size(2048, 2048);
        Size _atlasSize;
        Size _minSize = new Size(0, 0);
        int _paddingPixels = 1;
        private List<ImageData> _imageData = new List<ImageData>();					// Image data.
        private List<Image> _atlasImages = new List<Image>();					// Atlas images.
        private int _currentIndex = -1;								// Current image.
        private Size usedSize = new Size(0,0);
        private int usedPixels = 0;
        private double usedPixelCoefficient = 0;
        private float usedPixelCoefficientIncrement = 1.5f;
        private bool iterating = false;
        bool final = false;
        


        static void Main(string[] args)
        {
            AtlasCommandLineTool t = new AtlasCommandLineTool();
            t.ParseArgs(args);
        }

        public void ClearVariables()
        {
            _images = new Dictionary<string, Image>();
            _imageNames = new List<string>();
            _imageData = new List<ImageData>();					// Image data.
            _atlasImages = new List<Image>();					// Atlas images.
            _currentIndex = -1;								// Current image.
            usedSize = new Size(0,0);
            usedPixels = 0;
        }

        public void ParseArgs(string[] _args)
        {
            args = _args;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Substring(0, 1) == "-" && !checkedArgs.Contains(i))
                {
                    switch (args[i].Substring(1, 1))
                    {
                        case "h":
                            PrintHelp();
                            return;
                        case "n":
                            outputFile = args[i + 1];
                            checkedArgs.Add(i + 1);
                            break;
                        case "o":
                            outputDir = args[i + 1];
                            checkedArgs.Add(i + 1);
                            break;
                        case "i":
                            inputDir = args[i + 1];
                            checkedArgs.Add(i + 1);
                            break;
                    }
                    checkedArgs.Add(i);
                }
                else if (!checkedArgs.Contains(i))
                {
                    PrintHelp("Invalid Argument: " + args[i]);
                    return;
                }
            }
            string error = CheckOutputPath();
            if (error != null)
            {
                PrintHelp(error);
                return;
            }
            error = CheckInputPath();
            if (error != null)
            {
                PrintHelp(error);
                return;
            }
            else
            {
                _atlasSize = _maxAtlasSize;
                Execute();
            }
        }

        public string CheckOutputPath()
        {
            if(outputFile == null)
                return "No Output filename specified.";
            if (outputDir == null)
                return "No Output directory specified.";
            if (!System.IO.Directory.Exists(outputDir))
                return "Output directory does not exist: " + outputDir;
            return null;
        }

        public string CheckInputPath()
        {
            if (inputDir == null)
                return "No input directory specified.";
            if (!System.IO.Directory.Exists(inputDir))
                return "Input directory does not exist: " + inputDir;
            return null;
        }

        public void Execute()
        {
            if (!iterating)
            {
                Console.WriteLine("Input directory: " + inputDir);
                Console.WriteLine("Output directory: " + outputDir);
                Console.WriteLine("Output file: " + outputFile);
                Console.WriteLine("Starting Atlas.");
            }
            LoadTextures();
            GenerateAtlas();

            int unUsedPixels = (_atlasSize.Height * _atlasSize.Width) - usedPixels;

            if ((unUsedPixels > (usedPixels * 0.01)/* || (usedSize.Width < _atlasSize.Width / 2 || usedSize.Height < _atlasSize.Height / 2)*/) && _atlasImages.Count < 2 && !final)
            {

                /*if ((usedSize.Width < _atlasSize.Width - 1 || usedSize.Height < _atlasSize.Height - 1)) // If there's whitespace along the edges
                {
                    _atlasSize.Width = (int)Math.Ceiling((double)_atlasSize.Width * ((double)usedSize.Width / (double)_atlasSize.Width)) + 1;
                    _atlasSize.Height = (int)Math.Ceiling((double)_atlasSize.Height * ((double)usedSize.Height / (double)_atlasSize.Height)) + 1;
                }*/
                usedPixelCoefficient = (usedPixelCoefficient + 0.1) * usedPixelCoefficientIncrement;
                _atlasSize.Width = (int)Math.Floor(_maxAtlasSize.Width - Math.Sqrt(usedPixels * usedPixelCoefficient));
                _atlasSize.Height = (int)Math.Floor(_maxAtlasSize.Height - Math.Sqrt(usedPixels * usedPixelCoefficient));
              

                if (_atlasSize.Height < _minSize.Height)
                    _atlasSize.Height = _minSize.Height + 1;
                if (_atlasSize.Width < _minSize.Width)
                    _atlasSize.Width = _minSize.Width + 1;

                ClearVariables();
                Console.WriteLine("Atlas is much larger than input, reducing atlas size to " + _atlasSize.Width.ToString() + "x" + _atlasSize.Height.ToString());
                iterating = true;
                Execute();                
            }
            else if (_atlasImages.Count >= 2 && usedPixelCoefficient > 0 && !final)
            {
                usedPixelCoefficient = (usedPixelCoefficient / usedPixelCoefficientIncrement) - 0.1;
                _atlasSize.Width = (int)Math.Ceiling(_maxAtlasSize.Width - Math.Sqrt(usedPixels * usedPixelCoefficient));
                _atlasSize.Height = (int)Math.Ceiling(_maxAtlasSize.Height - Math.Sqrt(usedPixels * usedPixelCoefficient));

                if (_atlasSize.Height < _minSize.Height)
                    _atlasSize.Height = _minSize.Height + 1;
                if (_atlasSize.Width < _minSize.Width)
                    _atlasSize.Width = _minSize.Width + 1;

                ClearVariables();
                if (usedPixelCoefficientIncrement <= 1.15f)
                    final = true;
                else
                    usedPixelCoefficientIncrement -= 0.05f;
                iterating = true;
                Console.WriteLine("Made atlas too small, backing out to previous size: " + _atlasSize.Width.ToString() + "x" + _atlasSize.Height.ToString());
                Execute();
            }
            else
                SaveAtlas();
        }

        private void LoadTextures()
        {
            string[] textures = System.IO.Directory.GetFiles(inputDir, "*.png");
            if(!iterating)
                Console.WriteLine(textures.Count().ToString() + " files found.");
            foreach (string tex in textures)
            {
                _images.Add(tex,Image.FromFile(tex));
            }
            if(!iterating)
                Console.WriteLine(_images.Count.ToString() + " images loaded.");

            SortTextures();
        }

        /// <summary>
        /// Sort textures by size in descending order.
        /// </summary>
        public void SortTextures()
        {
            _sortedImageNames = (from i in _images
                                                             orderby i.Value.Height * i.Value.Width descending
                                                             select i.Key).ToList();
            _minSize = new Size(_images[_sortedImageNames[0]].Width, _images[_sortedImageNames[0]].Height);
        }

        public void PrintHelp(string error = "")
        {
            if(error != "")
                error = "\nError: " + error + "\n\n";
            Console.Write(@"AtlasTool V. 0.1
Copyright 2011 Robust Games, LLC
" + error +
@"Usage:
AtlasTool.exe -o atlasname <texpath>

<texpath> - full path to directory of textures to atlas
-n <atlasname> - output atlas name -- filename of .tai file and png files for atlas
-o <outpath> - output atlas directory -- directory to output tai and png atlas files to
-i <texpath> - input directory -- path to directory containing textures to atlas.
");
        }

        /// <summary>
        /// Function to generate the atlas.
        /// </summary>
        /// <param name="images">List of images to use as a source for an atlas image.</param>
        private void GenerateAtlas()
        {
            ImageTree tree = null;			// Image tree.
            Rectangle newPosition;			// New rectangle.
            Graphics g = null;				// Graphics context.
            ImageData data;					// Image data.
            Image atlas = null;				// Current atlas image.
            List<string> images = null;		// List of images.

            //Cursor.Current = Cursors.WaitCursor;
            try
            {
                //buttonSaveAtlas.Enabled = false;
                if (_images.Count < 1)
                    return;

                //Sort Images.
                

                /*Settings.Root = "AtlasSettings";
                _atlasSize.Width = Convert.ToInt32(Settings.GetSetting("MaxWidth", "4096"));
                _atlasSize.Height = Convert.ToInt32(Settings.GetSetting("MaxHeight", "4096"));
                
                _paddingPixels = Convert.ToInt32(Settings.GetSetting("Padding", "0"));*/

                // Reset.
                atlas = new Bitmap(_atlasSize.Width, _atlasSize.Height, PixelFormat.Format32bppArgb);

                // Begin adding to the image tree.
                tree = new ImageTree(_atlasSize);

                int currentIndex = 0;
                int firstSkipped = 0;

                images = new List<string>();
                foreach (string imageName in _sortedImageNames)
                    images.Add(imageName);

                // Add to tree.
                g = Graphics.FromImage(atlas);
                g.Clear(Color.FromArgb(0, 0, 0, 0));


                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                while (images.Count > 0)
                {
                    try
                    {
                        if (currentIndex >= images.Count)
                            currentIndex = 0;
                        Image image = _images[images[currentIndex]];		// Get image.
                        Size addSize = new Size(image.Width + _paddingPixels, image.Height + _paddingPixels);
                        newPosition = tree.Add(images[currentIndex], addSize);
                        if (newPosition != Rectangle.Empty)
                        {
                            usedPixels += addSize.Width * addSize.Height;

                            if (newPosition.Right > usedSize.Width)
                                usedSize.Width = newPosition.Right;
                            if (newPosition.Bottom > usedSize.Height)
                                usedSize.Height = newPosition.Bottom;
                            g.DrawImage(image, newPosition.Location.X, newPosition.Location.Y, image.Width, image.Height);

                            // Convert to data structure.
                            data = new ImageData();
                            data.Destination = string.Empty;
                            data.SourceName = images[currentIndex];
                            data.AtlasImage = atlas;
                            data.ScaledRect = new RectangleF((float)newPosition.X / (float)atlas.Width, (float)newPosition.Y / (float)atlas.Height,
                                        (float)(newPosition.Width - _paddingPixels) / (float)atlas.Width, (float)(newPosition.Height - _paddingPixels) / (float)atlas.Height);

                            _imageData.Add(data);
                            if (!_atlasImages.Contains(atlas))
                                _atlasImages.Add(atlas);
                        }
                        else
                        {
                            if (currentIndex == images.Count - 1) // Make a new atlas and reset counter to 0.
                            {
                                // Reset.
                                atlas = new Bitmap(_atlasSize.Width, _atlasSize.Height, PixelFormat.Format32bppArgb);

                                // make a new image tree
                                tree = new ImageTree(_atlasSize);

                                g = Graphics.FromImage(atlas);
                                g.Clear(Color.FromArgb(0, 0, 0, 0));

                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                                currentIndex = 0;
                            }
                            else // skip the current image
                            { 
                                currentIndex++; 
                            }

                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("The image '" + images[currentIndex] + "' is too large to fit into the constraints of the image.\nImage will be removed from the list.", ex.ToString());
                    }

                    images.RemoveAt(currentIndex);
                }

                if (_currentIndex == -1)
                    _currentIndex = 0;
                //buttonSaveAtlas.Enabled = true;
            }
            catch (Exception ex)
            {
                //UI.ErrorBox(this, "There was an error while trying to generate the atlas.", ex);
                Console.WriteLine("There was an error while trying to generate the atlas." + ex.ToString());
            }
            /*finally
            {
                this.Refresh();
                if (g != null)
                    g.Dispose();
                g = null;
                Cursor.Current = Cursors.Default;
                Settings.Root = null;
            }*/
        }

        /// <summary>
        /// Handles the Click event of the buttonSaveAtlas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SaveAtlas()
        {
            StreamWriter writer = null;		// Stream writer.

            try
            {
                Console.WriteLine("Saving Atlas to " + _atlasImages.Count.ToString() + " images.");
                writer = new StreamWriter(Path.ChangeExtension(outputDir + Path.DirectorySeparatorChar + outputFile, ".TAI"));

                for (int i = 0; i < _atlasImages.Count; i++)
                {
                    _atlasImages[i].Save(outputDir + Path.DirectorySeparatorChar + i.ToString() + "_" + outputFile + ".png");
                    foreach (ImageData item in _imageData)
                    {
                        if (item.AtlasImage == _atlasImages[i])
                            item.Destination = i.ToString() + "_" + outputFile + ".png";
                    }
                }

                // Write out index data.
                foreach (ImageData item in _imageData)
                {
                    string line = string.Empty;

                    line = item.SourceName + "\t\t" + item.Destination + ", 0, 2D, " + item.ScaledRect.X.ToString("0.000000", CultureInfo.InvariantCulture) + ", " +
                                item.ScaledRect.Y.ToString("0.000000", CultureInfo.InvariantCulture) + ", 0.000000, " + item.ScaledRect.Width.ToString("0.000000", CultureInfo.InvariantCulture) + ", " + item.ScaledRect.Height.ToString("0.000000", CultureInfo.InvariantCulture);

                    writer.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to save the atlas." + ex.ToString());
            }
            finally
            {
                Console.WriteLine("Atlas saved successfully.");
                if (writer != null)
                    writer.Dispose();
                writer = null;
            }
        }

    }

    /// <summary>
    /// Image data value type.
    /// </summary>
    class ImageData
    {
        /// <summary>
        /// Source image.
        /// </summary>
        public string SourceName;
        /// <summary>
        /// Destination image.
        /// </summary>
        public string Destination;
        /// <summary>
        /// Scaled rectangle.
        /// </summary>
        public RectangleF ScaledRect;
        /// <summary>
        /// Atlas image.
        /// </summary>
        public Image AtlasImage;
    }
}
