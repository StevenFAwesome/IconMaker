Imports System.IO
Imports System.Windows
' We keep the imports, but will use fully qualified names in the code below
Imports System.Drawing
Imports System.Drawing.Imaging

Class MainWindow

    ' Target size for the largest icon resolution (modern standard)
    Private Const TargetSize As Integer = 256

    Private Sub Window_DragEnter(sender As Object, e As DragEventArgs)
        ' Check if the dragged data contains file names
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            ' Assume copy operation is acceptable
            e.Effects = DragDropEffects.Copy
        Else
            e.Effects = DragDropEffects.None
        End If
        e.Handled = True
    End Sub

    Private Sub Window_Drop(sender As Object, e As DragEventArgs)
        ' Check if files were dropped
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            Dim convertedCount As Integer = 0
            Dim failedCount As Integer = 0

            StatusTextBlock.Text = "Processing..."

            For Each filePath In files
                Dim extension As String = Path.GetExtension(filePath).ToLower()

                ' Check for supported file types
                If extension = ".png" OrElse extension = ".jpg" OrElse extension = ".jpeg" Then
                    If ConvertImageToIcon(filePath) Then
                        convertedCount += 1
                    Else
                        failedCount += 1
                    End If
                Else
                    failedCount += 1
                End If
            Next

            ' Display results
            If convertedCount > 0 OrElse failedCount > 0 Then
                StatusTextBlock.Text = $"Conversion complete! Converted: {convertedCount}, Failed: {failedCount}. Icons saved next to source files."
                If failedCount > 0 Then
                    MessageBox.Show($"Warning: {failedCount} files failed or were not supported. Only PNG/JPG are converted.", "Conversion Result", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If
            Else
                StatusTextBlock.Text = "No valid PNG or JPG files were dropped."
            End If
        End If
    End Sub

    ''' <summary>
    ''' Loads an image, resizes it, and attempts to save it as an ICO file.
    ''' </summary>
    ''' <param name="sourcePath">The full path to the source image file.</param>
    ''' <returns>True if conversion was successful, False otherwise.</returns>
    'Private Function ConvertImageToIcon(sourcePath As String) As Boolean
    '    Try
    '        ' 1. Load the image using System.Drawing
    '        ' *** Using fully qualified name System.Drawing.Bitmap ***
    '        Using originalBitmap As New System.Drawing.Bitmap(sourcePath)

    '            ' 2. Create a resized version of the image for the icon (TargetSize x TargetSize)
    '            ' *** Using fully qualified name System.Drawing.Bitmap ***
    '            Dim resizedBitmap As New System.Drawing.Bitmap(TargetSize, TargetSize)

    '            ' *** Using fully qualified name System.Drawing.Graphics ***
    '            Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(resizedBitmap)
    '                ' Set interpolation mode for better quality resizing
    '                ' *** Using fully qualified name System.Drawing.Drawing2D.InterpolationMode ***
    '                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
    '                g.DrawImage(originalBitmap, 0, 0, TargetSize, TargetSize)
    '            End Using

    '            ' 3. Create the output path
    '            Dim directory As String = Path.GetDirectoryName(sourcePath)
    '            Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(sourcePath)
    '            Dim outputPath As String = Path.Combine(directory, $"{fileNameWithoutExt}.ico")

    '            ' 4. Convert the Bitmap to an Icon object
    '            ' GetHicon() creates a handle to the GDI icon, which System.Drawing.Icon can use.
    '            Dim iconHandle As IntPtr = resizedBitmap.GetHicon()

    '            ' *** Using fully qualified name System.Drawing.Icon ***
    '            Using icon As System.Drawing.Icon = System.Drawing.Icon.FromHandle(iconHandle)

    '                ' 5. Save the Icon to the file system
    '                Using fs As New FileStream(outputPath, FileMode.Create)
    '                    icon.Save(fs)
    '                End Using
    '            End Using

    '            ' Clean up the resized bitmap
    '            resizedBitmap.Dispose()

    '            Return True ' Success
    '        End Using

    '    Catch ex As Exception
    '        ' Log the error (optional, but good practice)
    '        System.Diagnostics.Debug.WriteLine($"Error converting {sourcePath}: {ex.Message}")

    '        ' Show a message box if a critical error occurs during conversion
    '        MessageBox.Show($"Failed to convert {Path.GetFileName(sourcePath)}. Error: {ex.Message}", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error)

    '        Return False ' Failure
    '    End Try
    'End Function
    Private Function ConvertImageToIcon(sourcePath As String) As Boolean
        Try
            Dim directory As String = Path.GetDirectoryName(sourcePath)
            Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(sourcePath)
            Dim outputPath As String =
                Path.Combine(directory, fileNameWithoutExt & ".ico")

            Using original As New System.Drawing.Bitmap(sourcePath)

                ' Create a high-quality 256x256 image with alpha transparency
                Using iconBitmap As New System.Drawing.Bitmap(
                    TargetSize,
                    TargetSize,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb)

                    Using g As System.Drawing.Graphics =
                        System.Drawing.Graphics.FromImage(iconBitmap)

                        ' Transparent background
                        g.Clear(System.Drawing.Color.Transparent)

                        ' High quality rendering
                        g.CompositingMode =
                            System.Drawing.Drawing2D.CompositingMode.SourceOver

                        g.CompositingQuality =
                            System.Drawing.Drawing2D.CompositingQuality.HighQuality

                        g.InterpolationMode =
                            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic

                        g.SmoothingMode =
                            System.Drawing.Drawing2D.SmoothingMode.HighQuality

                        g.PixelOffsetMode =
                            System.Drawing.Drawing2D.PixelOffsetMode.HighQuality

                        ' Maintain original aspect ratio
                        Dim scale As Double =
                            Math.Min(TargetSize / CDbl(original.Width),
                                     TargetSize / CDbl(original.Height))

                        Dim width As Integer =
                            CInt(original.Width * scale)

                        Dim height As Integer =
                            CInt(original.Height * scale)

                        ' Center the image
                        Dim x As Integer =
                            (TargetSize - width) \ 2

                        Dim y As Integer =
                            (TargetSize - height) \ 2

                        g.DrawImage(
                            original,
                            New System.Drawing.Rectangle(x, y, width, height),
                            0,
                            0,
                            original.Width,
                            original.Height,
                            System.Drawing.GraphicsUnit.Pixel)

                    End Using

                    ' Save the 256x256 image as PNG in memory
                    Using pngStream As New MemoryStream()

                        iconBitmap.Save(
                            pngStream,
                            System.Drawing.Imaging.ImageFormat.Png)

                        Dim pngData() As Byte = pngStream.ToArray()

                        ' Build a modern ICO containing the PNG
                        Using fs As New FileStream(
                            outputPath,
                            FileMode.Create,
                            FileAccess.Write)

                            Using writer As New BinaryWriter(fs)

                                ' -------------------------
                                ' ICO HEADER
                                ' -------------------------

                                ' Reserved
                                writer.Write(CUShort(0))

                                ' Type: 1 = Icon
                                writer.Write(CUShort(1))

                                ' Number of images
                                writer.Write(CUShort(1))


                                ' -------------------------
                                ' ICON DIRECTORY ENTRY
                                ' -------------------------

                                ' Width
                                ' 0 means 256 pixels
                                writer.Write(CByte(0))

                                ' Height
                                ' 0 means 256 pixels
                                writer.Write(CByte(0))

                                ' Color palette count
                                writer.Write(CByte(0))

                                ' Reserved
                                writer.Write(CByte(0))

                                ' Color planes
                                writer.Write(CUShort(1))

                                ' Bits per pixel
                                writer.Write(CUShort(32))

                                ' Size of PNG data
                                writer.Write(CUInt(pngData.Length))

                                ' Offset where image begins
                                ' ICO header = 6 bytes
                                ' Directory entry = 16 bytes
                                writer.Write(CUInt(22))

                                ' -------------------------
                                ' PNG IMAGE DATA
                                ' -------------------------

                                writer.Write(pngData)

                            End Using
                        End Using
                    End Using
                End Using
            End Using

            Return True

        Catch ex As Exception

            System.Diagnostics.Debug.WriteLine(
                $"Error converting {sourcePath}: {ex.Message}")

            MessageBox.Show(
                $"Failed to convert {Path.GetFileName(sourcePath)}." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Conversion Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error)

            Return False

        End Try
    End Function
End Class