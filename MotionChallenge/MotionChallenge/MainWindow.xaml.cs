using OpenTK;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionChallenge
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // GLControl component
        OpenTK.GLControl glControl;

        // Initialisation des variables liees aux traces OpenGL
        float cameraX = 0;
        float cameraY = -400;
        float cameraZ = 170;
        float cameraDirectionX = 0;
        float cameraDirectionY = 0;
        float cameraDirectionZ = 170;
        float cameraSpeed = 10;
        Matrix4 cameraLookAt;
        double wallWidth = 150;
        double wallHeight = 200;
        double wallDepth = 10;
        double wallY = 0;
        double groundMin = -400;
        double groundMax = 100;
        double groundWidth = 150;
        double zoom = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the GLControl component
            glControl = new OpenTK.GLControl();
            glControl.SetBounds(0, 0, 640, 480);

            // Add the component into the WindowsFormHost
            windowsFormsHost.Child = glControl;
            glControl.Paint += new PaintEventHandler(Paint);

            // Initialize OpenGL
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            
            // Texture loading with SOIL
            // TODO

        }

        private void Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Initialize OpenGL matrices
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Frustum(-1, 1, -1, 0.5, 1, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            
            // Camera configuration
            cameraLookAt = Matrix4.LookAt(cameraX, cameraY, cameraZ, cameraDirectionX, cameraDirectionY, cameraDirectionZ, 0.0f, 0.0f, 1.0f);
            GL.LoadMatrix(ref cameraLookAt);

            // Univers
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(150.0, 150.0, 150.0);

                // Face arriere gauche
                GL.Vertex3(-1000, 1, wallHeight * 4);
                GL.Vertex3(-wallWidth, 1, wallHeight * 4);
                GL.Vertex3(-wallWidth, 1, 0);
                GL.Vertex3(-1000, 1, 0);

                // Face arriere centrale (en avant)
                GL.Vertex3(-wallWidth, 1, wallHeight * 4);
                GL.Vertex3(wallWidth, 1, wallHeight * 4);
                GL.Vertex3(wallWidth, 1, wallHeight);
                GL.Vertex3(-wallWidth, 1, wallHeight);

                // Face arriere droite
                GL.Vertex3(wallWidth, 1, wallHeight * 4);
                GL.Vertex3(1000, 1, wallHeight * 4);
                GL.Vertex3(1000, 1, 0);
                GL.Vertex3(wallWidth, 1, 0);

                GL.Color3(Color.White);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(100.0, 100.0, 100.0);

                // Face arriere centrale (en arriere)
                GL.Vertex3(-wallWidth, 100, wallHeight);
                GL.Vertex3(wallWidth, 100, wallHeight);
                GL.Vertex3(wallWidth, 100, 0);
                GL.Vertex3(-wallWidth, 100, 0);

                GL.Color3(Color.White);
            GL.End();

            // Mur qui avance
            //glBindTexture(GL_TEXTURE_2D, texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                // Face arriere
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);

                // Face laterale gauche
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, 0);

                // Face laterale droite
                GL.TexCoord2(0, 1); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(wallWidth, wallY, 0);

                // Face interieure 1
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, 0);

                // Face interieure 2
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth / 2, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth / 2, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth / 2, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth / 2, 0);

                // Face interieure 2
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, 0);
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, 0);

                // Face frontale
                // TODO d -> i
                GL.Color3(Color.Green); GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.Color3(Color.Green); GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.Color3(Color.Green); GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY, 0);
                GL.Color3(Color.Green); GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, 0);

                GL.Color3(Color.White);
            GL.End();

            // Sol (trajectoire du mur)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Yellow);

                GL.Vertex3(-groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMin, 0);
                GL.Vertex3(-groundWidth, groundMin, 0);

                GL.Color3(Color.White);
            GL.End();

            // Sol (global)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Vertex3(-1000, groundMax, 0);
                GL.Vertex3(1000, groundMax, 0);
                GL.Vertex3(1000, groundMin, 0);
                GL.Vertex3(-1000, groundMin, 0);
            GL.End();

            GL.Flush();
            glControl.SwapBuffers();
        }
    }
}
