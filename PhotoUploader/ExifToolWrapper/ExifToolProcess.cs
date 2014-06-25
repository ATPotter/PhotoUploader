/* Copyright 2008 Robert C. Brinson <rbrinson@gmail.com>
 * 
 * The PictureTagger.ExifToolWrapper.ExifToolWrapper class is a C# port
 * of the ExifToolWrapper java class originally created by Wyatt Olson.
 * The java ExifToolWrapper class is part of the Moss project on
 * SourceForge.net <http://sourceforge.net/projects/moss/>.
 * 
 * This file is part of PictureTagger.
 *
 * PictureTagger is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 *
 * PictureTagger is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with PictureTagger.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Diagnostics;

namespace PictureTagger.ExifToolWrapper
{
        public class ExifToolProcess : IExifToolProcess
        {
                private FileInfo _exifToolFile = null;
                private Process _exifTool = null;
                
                public ExifToolProcess(FileInfo exifToolFile)
                {
                        if (File.Exists(exifToolFile.FullName) && (File.GetAttributes(exifToolFile.FullName) & FileAttributes.Directory) == 0)
                        {
                                _exifToolFile = exifToolFile;
                        }
                        else
                        {
                                throw new FileNotFoundException("The exiftool executable could not be found at the location supplied. Please check that exiftool is installed and is located at the following path: " + exifToolFile.FullName);
                        }
                }
                
                private Process GenerateExifToolProcess(string arguments)
                {
                        Process exifTooProcess = null;
                        ProcessStartInfo psi = new ProcessStartInfo();
                        
                        psi.FileName = _exifToolFile.FullName;
                        psi.Arguments = arguments;
                        psi.RedirectStandardOutput = true;
                        psi.UseShellExecute = false;
                        
                        try
                        {
                                exifTooProcess = Process.Start(psi);
                        }
                        catch (Exception e)
                        {
                                throw new ApplicationException("An error occurred while trying to execute exiftool with the following arguments: " + psi.Arguments, e);
                        }

                        return exifTooProcess;
                }
                
                public Stream GetBaseStreamFromExifToolCommand(string command)
                {
                        Stream exifToolStream = null;
                        try
                        {
                                _exifTool = GenerateExifToolProcess(command);
                                exifToolStream = _exifTool.StandardOutput.BaseStream;
                        }
                        catch (Exception)
                        {
                                throw;
                        }
                        
                        return exifToolStream;
                }
                
                public string GetStreamAsStringFromExifToolCommand(string command)
                {
                        string exifToolStream = String.Empty;
                        try
                        {
                                _exifTool = GenerateExifToolProcess(command);
                                exifToolStream = _exifTool.StandardOutput.ReadToEnd();
                        }
                        catch (Exception)
                        {
                                throw;
                        }
                        
                        return exifToolStream;
                }
                
                public void WaitForStreamExit()
                {
                        if (_exifTool != null)
                        {
                                _exifTool.WaitForExit(1000);
                        }
                }
        }
}
