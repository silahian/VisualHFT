using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using OxyPlot.Series;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel;

namespace VisualHFT.View
{
   
    /// <summary>
    /// Interaction logic for ucOrderBook.xaml
    /// </summary>
    public partial class ucOrderBook : UserControl
    {
        private struct GlyphInfo
        {
            public int TextureID;
            public float Width;
            public float Height;
            public float BearingX;
            public float BearingY;
            public float Advance;
        }

        private Dictionary<char, GlyphInfo> _glyphMap;
        private Library _ftLibrary;
        private Face _ftFace;

        private int _shaderProgram;
        private int _vao;
        private int _vbo;


        private DateTime _startTime;
        private float _timeWindow = 60; // Show last 60 seconds of data 
        public ucOrderBook()
        {
            InitializeComponent();

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 2,
                MinorVersion = 1,
                RenderContinuously = false,
            };
            _startTime = DateTime.Now;
            glControl.Start(settings);
            glControl.Render += OnRender;


            glControlRealTimeSpread.Start(settings);
            glControlRealTimeSpread.Render += GlControlRealTimeSpread_Render;

            string fontPath = @"C:\Windows\Fonts\arial.ttf";
            if (!File.Exists(fontPath))
            {
                MessageBox.Show($"Font not found: {fontPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            try
            {
                InitFreeType(fontPath,12);
                InitShader();
                InitBuffers();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

        }

        private void GlControlRealTimeSpread_Render(TimeSpan obj)
        {
            // Clear buffers
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            DateTime currentTime = DateTime.Now;
            DateTime windowStartTime = currentTime - TimeSpan.FromSeconds(_timeWindow);

            var realtimeprice = (ViewModel.vmOrderBook)this.DataContext;
            if (realtimeprice == null || realtimeprice.RealTimeSpreadModel == null || realtimeprice.RealTimeSpreadModel.Series == null)
                return;

            var data = realtimeprice.RealTimeSpreadModel.Series;

            List<(float X, double Y, DateTime dated)> visibleData = new List<(float X, double Y, DateTime dated)>();
            List<(float X, double Y, DateTime dated)> bidData = new List<(float X, double Y, DateTime dated)>();

            foreach (var item in data)
            { 
                if (item is OxyPlot.Series.LineSeries _scatter)
                {
                    visibleData = _scatter.Points
                             .Select(d =>
                             {
                                 float x = (float)(DateTime.FromOADate(d.X) - windowStartTime).TotalSeconds;
                                 return (X: x, d.Y, dated: DateTime.FromOADate(d.X));
                             })
                             .ToList();
                }
            }

                    

            float minX = 0;
            float maxX = _timeWindow;
            if (visibleData.Count == 0)
            {
                return;
            }
            // Dynamic Y-axis bounds based on data
            float minY = (float)Math.Min(visibleData.Min(p => p.Y), visibleData.Min(p => p.Y));
            float maxY = (float)Math.Max(visibleData.Max(p => p.Y), visibleData.Max(p => p.Y));


            List<double> ticks = GenerateAxisTicks(visibleData.Min(x => x.Y), visibleData.Max(x => x.Y), 30);
            List<DateTime> timeTicks = GenerateTimeTicks(visibleData.Min(x => x.dated), visibleData.Max(x => x.dated), 50);


            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(timeTicks));

            // Add padding to Y-axis
            float paddingY = (maxY - minY) * 0.1f; // 10% padding
            minY -= paddingY;
            maxY += paddingY;

            // Apply zoom factor
            float yCenter = (minY + maxY) / 2;
            float yRange = (maxY - minY);
            minY = yCenter - yRange / 2;
            maxY = yCenter + yRange / 2;

            // Set up projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(minX, maxX, minY, maxY, -1, 1);

            DrawAxesForLine(minX, maxX, minY, maxY, ticks,timeTicks, windowStartTime);


            // Draw the points
            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Lines); 
            GL.Color3(0.0f, 0.0f, 1.0f);

            for (int i = 1; i < visibleData.Count; i++)
            {
                GL.Vertex2(visibleData[i-1].X, visibleData[i-1].Y);
                GL.Vertex2(visibleData[i].X, visibleData[i].Y);
            }
            GL.End();


            int windowWidth = 1200;
            int windowHeight = 700;
            var textProjection = Matrix4.CreateOrthographicOffCenter(0, windowWidth, 0, windowHeight, -1, 1);
            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref textProjection);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "textColor"), 1.0f, 1.0f, 1.0f); // White color

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Bind the VAO for text rendering
            GL.BindVertexArray(_vao);


            DateTime lastPlotted = DateTime.MinValue;
            for (int i = 1; i < visibleData.Count; i++)
            {
                if ((visibleData[i].dated- lastPlotted).TotalSeconds > 10)
                {
                    float transformedX = (visibleData[i].X - minX) / (maxX - minX) * windowWidth;
                    DrawTextLabelForChart(visibleData[i].dated.ToString("HH:mm:ss"), transformedX, minY-40, 1.0f);
                    lastPlotted = visibleData[i].dated;
                } 

            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Disable blending if not needed for other rendering
            GL.Disable(EnableCap.Blend);


        }

        private void InitFreeType(string fontPath, int pixelSize)
        {
            try
            {
                _ftLibrary = new Library();
                _ftFace = _ftLibrary.NewFace(fontPath, 0);
                _ftFace.SetPixelSizes(0, (uint)pixelSize);

                _glyphMap = new Dictionary<char, GlyphInfo>();
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                for (char c = (char)32; c < (char)127; c++)
                {
                    try
                    {
                        _ftFace.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);
                        var bitmap = _ftFace.Glyph.Bitmap;

                        if (bitmap.Buffer == null || bitmap.Width == 0 || bitmap.Rows == 0)
                        {
                             
                            continue;
                        }

                        int texId = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, texId);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
                                      bitmap.Width, bitmap.Rows, 0,
                                      PixelFormat.Red, PixelType.UnsignedByte, bitmap.Buffer);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                        _glyphMap[c] = new GlyphInfo
                        {
                            TextureID = texId,
                            Width = bitmap.Width,
                            Height = bitmap.Rows,
                            BearingX = _ftFace.Glyph.BitmapLeft,
                            BearingY = _ftFace.Glyph.BitmapTop,
                            Advance = _ftFace.Glyph.Advance.X.Value >> 6
                        };
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load glyph for '{c}': {ex.Message}");
                    }
                }

                if (_glyphMap.Count == 0)
                {
                    throw new Exception("No glyphs were loaded. Check the font file.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"FreeType initialization failed: {ex.Message}");
            }
        }

        private void InitShader()
        {
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aTex;
                out vec2 vTex;
                uniform mat4 projection;
                void main()
                {
                    gl_Position = projection * vec4(aPos, 0.0, 1.0);
                    vTex = aTex;
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                in vec2 vTex;
                out vec4 FragColor;
                uniform sampler2D textTex;
                uniform vec3 textColor;
                void main()
                {
                    float alpha = texture(textTex, vTex).r;
                    if (alpha < 0.1) discard;
                    FragColor = vec4(textColor, alpha);
                }
            ";

            _shaderProgram = GL.CreateProgram();
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderCode);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderCode);

            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string code)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"Failed to compile {type}: {log}");
            }
            return shader;
        }

        private void InitBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        private void OnRender(TimeSpan delta)
        {
            // Clear buffers
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f); 

            DateTime currentTime = DateTime.Now;
            DateTime windowStartTime = currentTime - TimeSpan.FromSeconds(_timeWindow);

            var realtimeprice = (ViewModel.vmOrderBook)this.DataContext;
            if (realtimeprice == null || realtimeprice.RealTimePricePlotModel == null || realtimeprice.RealTimePricePlotModel.Series == null)
                return;

            var data = realtimeprice.RealTimePricePlotModel.Series;

            List<(float X, double Y, DateTime dated)> visibleData = new List<(float X, double Y, DateTime dated)>();
            List<(float X, double Y, DateTime dated)> bidData = new List<(float X, double Y, DateTime dated)>();

            foreach (var item in data)
            {
                if (item is OxyPlot.Series.ScatterSeries _scatter)
                {
                    if (item.Title == "ScatterAsks")
                    {
                        visibleData = _scatter.Points
                            .Select(d =>
                            {
                                float x = (float)(DateTime.FromOADate(d.X) - windowStartTime).TotalSeconds;
                                return (X: x, d.Y,dated:DateTime.FromOADate(d.X));
                            })
                            .ToList();
                    }
                    else if (item.Title == "ScatterBids")
                    {
                        bidData = _scatter.Points
                            .Select(d =>
                            {
                                float x = (float)(DateTime.FromOADate(d.X) - windowStartTime).TotalSeconds;
                                return (X: x, d.Y, dated: DateTime.FromOADate(d.X));
                            })
                            .ToList();
                    }
                }
            }




            // Fixed X-axis bounds (carousel effect)
            float minX = 0;
            float maxX = _timeWindow;

            if (bidData.Count == 0)
            {
                return;
            }
            // Dynamic Y-axis bounds based on data
            float minY = (float)Math.Min(visibleData.Min(p => p.Y), bidData.Min(p => p.Y));
            float maxY = (float)Math.Max(visibleData.Max(p => p.Y), bidData.Max(p => p.Y));


            List<double> ticks = GenerateAxisTicks(bidData.Min(x => x.Y), visibleData.Max(x => x.Y), 30);
            var timeTicks = GenerateTimeTicks(visibleData.Min(x => x.dated), visibleData.Max(x => x.dated), 50);


            Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(timeTicks));

            // Add padding to Y-axis
            float paddingY = (maxY - minY) * 0.1f; // 10% padding
            minY -= paddingY;
            maxY += paddingY;

            // Apply zoom factor
            float yCenter = (minY + maxY) / 2;
            float yRange = (maxY - minY);
            minY = yCenter - yRange / 2;
            maxY = yCenter + yRange / 2;

            // Set up projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(minX, maxX, minY, maxY, -1, 1);

            DrawAxes(minX, maxX, minY, maxY, ticks);


            // Draw the points
            GL.PointSize(5.0f);
            GL.Begin(PrimitiveType.Points);

            // Draw ScatterAsks
            GL.Color3(1.0f, 0.0f, 0.0f); // Red

            foreach (var point in visibleData)
            {
                GL.Vertex2(point.X, point.Y);
            }
            GL.End();

            // Draw ScatterBids
            GL.Begin(PrimitiveType.Points);
            GL.Color3(0.0f, 1.0f, 0.0f); // Green
            foreach (var point in bidData)
            { 
                GL.Vertex2(point.X, point.Y);
            }
            GL.End(); 
        }

        private void DrawTextLabelForChart(string text, float x, float y, float scale)
        {
            GL.BindVertexArray(_vao);

            // Align to pixel grid
            x = (float)Math.Floor(x + 0.5f);
            y = (float)Math.Floor(y + 0.5f);

            foreach (char c in text)
            {
                if (!_glyphMap.TryGetValue(c, out var glyph))
                    continue;

                float xpos = x + glyph.BearingX * scale;
                float ypos = y - (glyph.Height - glyph.BearingY) * scale;
                float w = glyph.Width * scale;
                float h = glyph.Height * scale;

                float[] vertices = {
            xpos,     ypos + h,  0.0f, 0.0f,
            xpos,     ypos,      0.0f, 1.0f,
            xpos + w, ypos,      1.0f, 1.0f,
            xpos,     ypos + h,  0.0f, 0.0f,
            xpos + w, ypos,      1.0f, 1.0f,
            xpos + w, ypos + h,  1.0f, 0.0f
        };

                GL.BindTexture(TextureTarget.Texture2D, glyph.TextureID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += glyph.Advance * scale;
            }

            GL.BindVertexArray(0);
        }
        private void DrawText(string text, float x, float y, float scale)
        {
            //GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(_vao);

            foreach (char c in text)
            {
                if (!_glyphMap.TryGetValue(c, out var glyph))
                    continue;

                float xpos = x + glyph.BearingX * scale;
                float ypos = y - (glyph.Height - glyph.BearingY) * scale;
                float w = glyph.Width * scale;
                float h = glyph.Height * scale;

                float[] vertices = {
                    xpos,     ypos + h,  0.0f, 0.0f,
                    xpos,     ypos,      0.0f, 1.0f,
                    xpos + w, ypos,      1.0f, 1.0f,
                    xpos,     ypos + h,  0.0f, 0.0f,
                    xpos + w, ypos,      1.0f, 1.0f,
                    xpos + w, ypos + h,  1.0f, 0.0f
                };

                GL.BindTexture(TextureTarget.Texture2D, glyph.TextureID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += glyph.Advance * scale;
            }

            //GL.BindVertexArray(0);
        }
        private void DrawAxes(float minX, float maxX, float minY, float maxY, List<double>axes)
        {
            DrawAllTexts(axes,minX,maxX,minY,maxY);
            GL.UseProgram(0); 

            int count = 1;
            foreach (double item in axes)
            {
                if (count % 4 == 0)
                {
                    GL.LineWidth(3.0f);
                }
                else
                {
                    GL.LineWidth(2.0f);
                }
                GL.Color3(1f, 1f, 1f); // Light gray color
                GL.Begin(PrimitiveType.Lines);  
                if (count % 4 == 0)
                {
                    GL.Vertex2(2.5, item); // Start at the bottom edge
                    GL.Vertex2(2.8, item); // End at the top edge 
                }
                else
                {
                    GL.Vertex2(2.5, item); // Start at the bottom edge
                    GL.Vertex2(2.7, item); // End at the top edge 
                }
                GL.End();
                count++;
            } 
            GL.End(); 
        }
        private void DrawAxesForLine(float minX, float maxX, float minY, float maxY, List<double> axes, List<DateTime> timeTicks, DateTime windowStartTime)
        {
            DrawAllTextsLine(axes, minX, maxX, minY, maxY);
            GL.UseProgram(0);

            int count = 1;
            foreach (double item in axes)
            {
                if (count % 4 == 0)
                {
                    GL.LineWidth(3.0f);
                }
                else
                {
                    GL.LineWidth(2.0f);
                }
                GL.Color3(1f, 1f, 1f); // Light gray color
                GL.Begin(PrimitiveType.Lines);
                if (count % 4 == 0)
                {
                    GL.Vertex2(2.5, item); // Start at the bottom edge
                    GL.Vertex2(2.8, item); // End at the top edge 
                }
                else
                {
                    GL.Vertex2(2.5, item); // Start at the bottom edge
                    GL.Vertex2(2.7, item); // End at the top edge 
                }
                GL.End();
                count++;
            }

            GL.LineWidth(3.0f);
            GL.Color3(1f, 1f, 1f); // Light gray color
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(2.5, axes.Min()); // Start at the bottom edge
            GL.Vertex2(maxX, axes.Min()); // End at the top edge 
            GL.End();   
        }

        
        private void DrawAllTexts(List<double> ticks, float minX, float maxX, float minY, float maxY)
        {
            int windowWidth = 1200;
            int windowHeight = 700;
            var textProjection = Matrix4.CreateOrthographicOffCenter(0, windowWidth, 0, windowHeight, -1, 1);

            // Use the text shader program
            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref textProjection);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "textColor"), 1.0f, 1.0f, 1.0f); // White color
             
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Bind the VAO for text rendering
            GL.BindVertexArray(_vao);

            int count = 1;
            foreach (double item in ticks)
            {
                if (count % 4 == 0)
                { 
                    float yData = (float)item; 
                    float yNormalized = (yData - minY) / (maxY - minY) * windowHeight;

                    // Choose an x position, e.g., 10 pixels from the left
                    float xPos = 5f;
                    float yPos = yNormalized-5.0f; // Adjust yPos as needed for text height

                    // Example: Draw the tick value as text
                    string tickText = item.ToString();

                    DrawText(tickText, xPos, yPos, 1.0f);
                }
                count++;
            }

            // Unbind the VAO and shader program after text rendering
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Disable blending if not needed for other rendering
            GL.Disable(EnableCap.Blend);
        }
        private void DrawAllTextsLine(List<double> ticks, float minX, float maxX, float minY, float maxY)
        {
            int windowWidth = 1200;
            int windowHeight = 700;
            var textProjection = Matrix4.CreateOrthographicOffCenter(0, windowWidth, 0, windowHeight, -1, 1);

            // Use the text shader program
            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref textProjection);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "textColor"), 1.0f, 1.0f, 1.0f); // White color

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Bind the VAO for text rendering
            GL.BindVertexArray(_vao);

            int count = 1;
            foreach (double item in ticks)
            {
                if (count % 4 == 0)
                {
                    float yData = (float)item;
                    float yNormalized = (yData - minY) / (maxY - minY) * windowHeight;

                    // Choose an x position, e.g., 10 pixels from the left
                    float xPos = 5f;
                    float yPos = yNormalized - 5.0f; // Adjust yPos as needed for text height

                    // Example: Draw the tick value as text
                    string tickText = item.ToString();

                    DrawText(tickText, xPos, yPos, 1.0f);
                }
                count++;
            }

            // Unbind the VAO and shader program after text rendering
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Disable blending if not needed for other rendering
            GL.Disable(EnableCap.Blend);
        }


        private void butPopPriceChart_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(chtPrice, (Button)sender, newViewModel);
        }

        private void butPopSpreadChart_Click(object sender, RoutedEventArgs e)
        {
            //glControl.InvalidateVisual();

            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(glControl, (Button)sender, newViewModel);
        }

        private void butPopSymbol_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            HelperCommon.CreateCommonPopUpWindow(grdSymbol, (Button)sender, newViewModel, "Symbol", 450, 600);
        }

        private void butDepthView1_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 0;
        }

        private void butDepthView2_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 1;
        }

        private void butDepthView3_Click(object sender, RoutedEventArgs e)
        {
            var newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.SwitchView = 2;
        }

        private void butPopImbalances_Click(object sender, RoutedEventArgs e)
        {
            var currentViewModel = (ViewModel.vmOrderBook)this.DataContext;
            // Create model
            var newViewModel = new ViewModel.vmOrderBookFlowAnalysis(Helpers.HelperCommon.GLOBAL_DIALOGS);
            newViewModel.SelectedSymbol = currentViewModel.SelectedSymbol;
            newViewModel.SelectedLayer = currentViewModel.SelectedLayer;
            newViewModel.SelectedProvider = currentViewModel.SelectedProvider;
        }

        vmOrderBook newViewModel;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            newViewModel = (ViewModel.vmOrderBook)this.DataContext;
            newViewModel.PropertyChanged += NewViewModel_PropertyChanged; ;
        }
        private void NewViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

           glControl.InvalidateVisual();
           glControlRealTimeSpread.InvalidateVisual();

        }

        public static List<DateTime> GenerateTimeTicks(DateTime start, DateTime end, int maxTicks)
        {
            List<DateTime> ticks = new List<DateTime>();

            // Handle edge case where start == end
            if (start >= end)
            {
                ticks.Add(start);
                return ticks;
            }
             
            double totalSeconds = (end - start).TotalSeconds;
             
            double roughStep = totalSeconds / (maxTicks - 1);
            double stepSeconds;

            if (roughStep <= 10)
            {
                // Minimum step size is 30 seconds
                stepSeconds = 10;
            }
            else if (roughStep <= 3600)
            {
                // Use minutes for medium ranges
                stepSeconds = Math.Ceiling(roughStep / 60) * 60; // Round to nearest minute
            }
            else
            {
                // Use hours for large ranges
                stepSeconds = Math.Ceiling(roughStep / 3600) * 3600; // Round to nearest hour
            }

            // Generate ticks
            DateTime currentTick = start;
            while (currentTick <= end)
            {
                ticks.Add(currentTick);
                currentTick = currentTick.AddSeconds(stepSeconds);
            }

            // Ensure the last tick is exactly `end` if it doesn't already exist
            if (ticks[ticks.Count - 1] != end)
            {
                ticks.Add(end);
            }

            return ticks;
        }
        static List<double> GenerateAxisTicks(double min, double max, int maxTicks)
        {
            List<double> ticks = new List<double>();

            // Handle edge case where min == max
            if (min == max)
            {
                ticks.Add(min);
                return ticks;
            }

            // Calculate the range and rough step size
            double range = max - min;
            double roughStep = range / (maxTicks - 1);

            // Determine a "nice" step size (round to 1, 2, 5, 10, etc.)
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughStep))); // Base magnitude
            double[] niceSteps = { 1, 2, 5, 10 };
            double step = magnitude * niceSteps[0];

            foreach (var s in niceSteps)
            {
                if (roughStep <= magnitude * s)
                {
                    step = magnitude * s;
                    break;
                }
            }

            // Calculate the start and end of the axis, rounded to step size
            double axisStart = Math.Floor(min / step) * step;
            double axisEnd = Math.Ceiling(max / step) * step;

            // Generate ticks
            for (double value = axisStart; value <= axisEnd; value += step)
            {
                ticks.Add(Math.Round(value, 2)); // Round to 2 decimal places for stock prices
            }

            return ticks;
        }

    }
}
