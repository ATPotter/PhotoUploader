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

namespace PictureTagger.ExifToolWrapper
{
    public interface IExifToolProcess
    {
        Stream GetBaseStreamFromExifToolCommand(string command);
        string GetStreamAsStringFromExifToolCommand(string command);
        void WaitForStreamExit();
    }
}