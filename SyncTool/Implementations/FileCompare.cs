using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SyncTool.Implementations
{
    class FileCompare
    {
                // This method accepts two strings the represent two files to
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the
        // files are not the same.
        // from https://docs.microsoft.com/de-DE/troubleshoot/dotnet/csharp/create-file-compare
        public bool Compare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
    

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using FileStream fs1 = new FileStream(file1, FileMode.Open);
            using FileStream fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            const int BYTES_TO_READ = 1024;
            byte[] one = new byte[BYTES_TO_READ];
            byte[] two = new byte[BYTES_TO_READ];

            do
            {
                // Read one byte from each file.
                file1byte = fs1.Read(one,0,BYTES_TO_READ);
                file2byte = fs2.Read(two,0,BYTES_TO_READ);
            }
            while (((ReadOnlySpan<byte>) one).SequenceEqual((ReadOnlySpan<byte>) two) && (file1byte != 0));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1byte - file2byte) == 0);
        }
            }
        }
