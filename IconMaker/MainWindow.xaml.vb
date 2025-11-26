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
    Private Function ConvertImageToIcon(sourcePath As String) As Boolean
        Try
            ' 1. Load the image using System.Drawing
            ' *** Using fully qualified name System.Drawing.Bitmap ***
            Using originalBitmap As New System.Drawing.Bitmap(sourcePath)

                ' 2. Create a resized version of the image for the icon (TargetSize x TargetSize)
                ' *** Using fully qualified name System.Drawing.Bitmap ***
                Dim resizedBitmap As New System.Drawing.Bitmap(TargetSize, TargetSize)

                ' *** Using fully qualified name System.Drawing.Graphics ***
                Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(resizedBitmap)
                    ' Set interpolation mode for better quality resizing
                    ' *** Using fully qualified name System.Drawing.Drawing2D.InterpolationMode ***
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                    g.DrawImage(originalBitmap, 0, 0, TargetSize, TargetSize)
                End Using

                ' 3. Create the output path
                Dim directory As String = Path.GetDirectoryName(sourcePath)
                Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(sourcePath)
                Dim outputPath As String = Path.Combine(directory, $"{fileNameWithoutExt}.ico")

                ' 4. Convert the Bitmap to an Icon object
                ' GetHicon() creates a handle to the GDI icon, which System.Drawing.Icon can use.
                Dim iconHandle As IntPtr = resizedBitmap.GetHicon()

                ' *** Using fully qualified name System.Drawing.Icon ***
                Using icon As System.Drawing.Icon = System.Drawing.Icon.FromHandle(iconHandle)

                    ' 5. Save the Icon to the file system
                    Using fs As New FileStream(outputPath, FileMode.Create)
                        icon.Save(fs)
                    End Using
                End Using

                ' Clean up the resized bitmap
                resizedBitmap.Dispose()

                Return True ' Success
            End Using

        Catch ex As Exception
            ' Log the error (optional, but good practice)
            System.Diagnostics.Debug.WriteLine($"Error converting {sourcePath}: {ex.Message}")

            ' Show a message box if a critical error occurs during conversion
            MessageBox.Show($"Failed to convert {Path.GetFileName(sourcePath)}. Error: {ex.Message}", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error)

            Return False ' Failure
        End Try
    End Function

End Class