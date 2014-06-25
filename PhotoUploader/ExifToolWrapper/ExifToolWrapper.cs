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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PictureTagger.PictureTaggerUtility;

namespace PictureTagger.ExifToolWrapper
{
    public class ExifToolWrapper : IExifToolWrapper
    {
        //private FileInfo _exifTool;
        private IExifToolProcess _exifTool;
        private bool _rwValuesAsNumbers = true;

        //When printing the values of multiple files, this is the 
        // start of the line which identifies the file name.
        private const string EXIFTOOL_FILE_HEADER = "========";
        private const string VALUES_AS_NUMBERS = " -n";

        /// <summary>
        /// Creates a new ExifTool wrapper, pointing to the ExifTool command
        /// line file at the given location.
        /// </summary>
        /// <param name="exifTool">
        /// A <see cref="FileInfo"/>
        /// </param>
        public ExifToolWrapper(FileInfo exifTool)
            : this(new ExifToolProcess(exifTool))
        { }

        /// <summary>
        /// Creates a new ExifTool wrapper, given to an interface to ExifToolProcess.
        /// </summary>
        /// <param name="exifToolProcess">
        /// A <see cref="IExifToolProcess"/>
        /// </param>
        public ExifToolWrapper(IExifToolProcess exifToolProcess)
        {
            _exifTool = exifToolProcess;
        }

        public bool ReadWriteValuesAsNumbers
        {
            get { return _rwValuesAsNumbers; }
            set { _rwValuesAsNumbers = value; }
        }

        /// <summary>
        /// Returns a byte array of a binary tag with the given name.  Can be
        /// used to load embedded thumbnails, etc
        /// </summary>
        /// <param name="image">
        /// A <see cref="FileInfo"/>
        /// </param>
        /// <param name="tagName">
        /// A <see cref="System.String"/>
        /// </param>
        /// <param name="maxSize">
        /// A <see cref="System.Int32"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Byte"/>
        /// </returns>
        public byte[] GetBinaryTagFromFile(FileInfo image, string tagName, int maxSize)
        {
            byte[] tagValue = new byte[maxSize];
            StringBuilder command = new StringBuilder();

            command.Append("-b");
            command.Append(" -" + tagName);
            command.Append(" ");
            command.Append(image.FullName);

            try
            {
                BufferedStream binTag = new BufferedStream(_exifTool.GetBaseStreamFromExifToolCommand(command.ToString()));
                _exifTool.WaitForStreamExit();

                int offset = 0;
                int ret = 0;
                int bufferSize = 1024;
                byte[] temp = new byte[bufferSize];

                while ((ret = binTag.Read(temp, 0, temp.Length)) != 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        tagValue[offset + i] = temp[i];
                    }
                    offset += ret;
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return tagValue;
        }

        public IDictionary<FileInfo, byte[]> GetBinaryTagFromFiles(FileInfo[] files, string tagName)
        {
            IDictionary<FileInfo, byte[]> binaryTags = new Dictionary<FileInfo, byte[]>();
            IList<string> tagNames = new List<string>();

            tagNames.Add(tagName);
            IDictionary<string, IDictionary<string, string>> sizesRaw = GetTagsFromFiles(files, tagNames);
            IDictionary<FileInfo, Nullable<Int32>> tagSizeByFile = new Dictionary<FileInfo, Nullable<Int32>>();

            StringBuilder command = new StringBuilder();

            command.Append("-b");
            command.Append(" -" + tagName);
            foreach (FileInfo file in files)
            {
                command.Append(" ");
                command.Append(file.FullName);

                IDictionary<string, string> sizeSet = sizesRaw[file.FullName];
                if (sizeSet != null)
                {
                    string bufferSizeString = "0";
                    if (sizeSet.ContainsKey(tagName))
                    {
                        bufferSizeString = sizeSet[tagName];
                    }
                    if (bufferSizeString != null)
                    {
                        bufferSizeString = Regex.Replace(bufferSizeString, "\\D", "");
                        if (bufferSizeString != null && bufferSizeString.Length > 0)
                        {
                            int bufferSize = Int32.Parse(bufferSizeString);
                            tagSizeByFile.Add(file, bufferSize);
                        }
                    }
                }
            }

            try
            {
                BufferedStream binTag = new BufferedStream(_exifTool.GetBaseStreamFromExifToolCommand(command.ToString()));
                _exifTool.WaitForStreamExit();

                foreach (FileInfo file in files)
                {
                    if (tagSizeByFile[file].HasValue)
                    {
                        int bufferSize = tagSizeByFile[file].Value;
                        byte[] binaryTag = ReadBytes(binTag, bufferSize);
                        binaryTags.Add(file, binaryTag);
                    }
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return binaryTags;
        }

        /// <summary>
        /// Returns a IDictionary of the given tag names and values for all given
        /// files.  This can be used for all non-binary tags that are 
        /// given via ExifTool.
        /// </summary>
        /// <param name="images">
        /// A <see cref="IList`1"/>
        /// </param>
        /// <param name="tagNames">
        /// A <see cref="IList`1"/>
        /// </param>
        /// <returns>
        /// A <see cref="IDictionary`2"/>
        /// </returns>
        public IDictionary<string, IDictionary<string, string>> GetTagsFromFiles(FileInfo[] images, IList<string> tagNames)
        {
            IDictionary<string, IDictionary<string, string>> fileToTagValues = new Dictionary<string, IDictionary<string, string>>();

            if (images.Length == 0)
            {
                return fileToTagValues;
            }

            StringBuilder command = new StringBuilder();

            command.Append("-S");
            if (ReadWriteValuesAsNumbers)
            {
                command.Append(VALUES_AS_NUMBERS);
            }
            foreach (string tagName in tagNames)
            {
                command.Append(" -" + tagName);
            }
            foreach (FileInfo image in images)
            {
                command.Append(" " + image.FullName);
            }

            try
            {
                TextReader br = new StringReader(_exifTool.GetStreamAsStringFromExifToolCommand(command.ToString()));
                _exifTool.WaitForStreamExit();

                Regex rgx;
                string line;
                string currentFile = null;
                string value;

                if (images.Length == 1)
                {
                    currentFile = images[0].FullName;
                    value = null;
                    fileToTagValues.Add(currentFile, new Dictionary<string, string>());
                }

                while (true)
                {
                    line = br.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    value = null;
                    if (Regex.IsMatch(line, EXIFTOOL_FILE_HEADER + ".*"))
                    {
                        rgx = new Regex(EXIFTOOL_FILE_HEADER);
                        currentFile = rgx.Replace(line, "", 1).Trim();
                        value = null;
                        fileToTagValues.Add(currentFile, new Dictionary<string, string>());
                    }
                    else if (Regex.IsMatch(line, "[^:]+:.+"))
                    {
                        rgx = new Regex("[^:]+:");
                        value = rgx.Replace(line, "", 1).Trim();
                    }
                    else
                    {
                        value = null;
                    }

                    string[] split = line.Split(':');
                    string tag = split[0].Trim();

                    if (currentFile != null)
                    {
                        if (fileToTagValues[currentFile] != null)
                        {
                            if (tagNames.Contains(tag))
                            {
                                fileToTagValues[currentFile].Add(tag, value);
                            }
                        }
                    }
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return fileToTagValues;
        }

        public IDictionary<string, string> GetTagsFromFile(FileInfo image, IList<string> tagNames)
        {
            FileInfo[] file = new FileInfo[] { image };
            return GetTagsFromFiles(file, tagNames)[file[0].FullName];
        }

        /// <summary>
        /// Sets the given tags to the file. 
        /// </summary>
        /// <param name="images">
        /// A <see cref="FileInfo"/>
        /// </param>
        /// <param name="tags">
        /// A <see cref="IDictionary`2"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public string SetTagsToFiles(FileInfo[] images, IDictionary<string, string> tags)
        {
            StringBuilder returnValue = new StringBuilder();
            StringBuilder command = new StringBuilder();

            command.Append("-overwrite_original_in_place");
            if (ReadWriteValuesAsNumbers)
            {
                command.Append(VALUES_AS_NUMBERS);
            }
            foreach (string tagName in tags.Keys)
            {
                if (tagName == ExifToolTags.IPTC_KEYWORDS || tagName == ExifToolTags.XMP_SUBJECT)
                {
                    string[] keywords = tags[tagName].Split(',');
                    foreach (string keyword in keywords)
                    {
                        command.Append(" -" + tagName + "=\"" + keyword.Trim() + "\"");
                    }
                }
                else
                {
                    command.Append(" -" + tagName + "=\"" + tags[tagName] + "\"");
                }
            }
            foreach (FileInfo image in images)
            {
                command.Append(" " + image.FullName);
            }

            try
            {
                TextReader br = new StringReader(_exifTool.GetStreamAsStringFromExifToolCommand(command.ToString()));
                _exifTool.WaitForStreamExit();

                string line;
                while ((line = br.ReadLine()) != null)
                {
                    returnValue.Append(line).Append("\n");
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return returnValue.ToString();
        }

        public string SetTagsToFile(FileInfo image, IDictionary<string, string> tags)
        {
            FileInfo[] file = new FileInfo[] { image };
            return SetTagsToFiles(file, tags);
        }

        /// <summary>
        /// Sets the given tags in the scpecified images using a data file.  We do 
        /// ABSOLUTELY NO SANITY CHECKS that the tags you specify can be
        /// properly read in from the files you specify. Be sure you check 
        /// that you are writing the correct values to the correct images,
        /// or you will lose data! 
        /// </summary>
        /// <param name="images">
        /// A <see cref="FileInfo"/>
        /// </param>
        /// <param name="tags">
        /// A <see cref="IDictionary`2"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public string SetTagsToFilesFromDataFile(FileInfo[] images, IDictionary<string, FileInfo> tags)
        {
            StringBuilder returnValue = new StringBuilder();
            StringBuilder command = new StringBuilder();

            command.Append("-overwrite_original_in_place");
            if (ReadWriteValuesAsNumbers)
            {
                command.Append(VALUES_AS_NUMBERS);
            }
            foreach (string tagName in tags.Keys)
            {
                command.Append(" -" + tagName + "<=" + tags[tagName].FullName);
            }
            foreach (FileInfo image in images)
            {
                command.Append(" " + image.FullName);
            }

            try
            {
                TextReader br = new StringReader(_exifTool.GetStreamAsStringFromExifToolCommand(command.ToString()));
                _exifTool.WaitForStreamExit();

                string line;
                while ((line = br.ReadLine()) != null)
                {
                    returnValue.Append(line).Append("\n");
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return returnValue.ToString();
        }

        public string SetTagsToFileFromDataFile(FileInfo image, IDictionary<string, FileInfo> tags)
        {
            FileInfo[] file = new FileInfo[] { image };
            return SetTagsToFilesFromDataFile(file, tags);
        }

        /// <summary>
        /// Reads the specified number of bytes from the buffered input stream,
        /// starting at offset.
        /// </summary>
        /// <param name="bis">
        /// A <see cref="BufferedStream"/>
        /// </param>
        /// <param name="length">
        /// A <see cref="System.Int32"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Byte"/>
        /// </returns>
        private byte[] ReadBytes(BufferedStream bis, int length)
        {
            byte[] binaryTag = new byte[length];

            for (int i = 0; i < length; i++)
            {
                binaryTag[i] = (byte)bis.ReadByte();
            }

            return binaryTag;
        }
    }
}